using Cosmos.HAL.Drivers.Video.SVGAII;
using PrismAPI.Graphics;
using Cosmos.Core;
using Cosmos.HAL;

namespace PrismAPI.Hardware.GPU.VMWare;

/// <summary>
/// The VMWare SVGAII canvas class. Allows for fast(er) graphics.
/// </summary>
public unsafe class SVGAIICanvas : Display
{
	#region Constructors

	/// <summary>
	/// Creates a new instance of the <see cref="SVGAIICanvas"/> class.
	/// </summary>
	/// <param name="Width">Total width of the display.</param>
	/// <param name="Height">Total height of the display.</param>
	public SVGAIICanvas(ushort Width, ushort Height) : base(0, 0) // Use 0 so no data is assigned.
	{
		Device = PCI.GetDevice(VendorID.VMWare, DeviceID.SVGAIIAdapter);
		Device.EnableMemory(true);

		uint BasePort = Device.BaseAddressBar[0].BaseAddress;
		IndexPort = (ushort)(BasePort + (uint)IOPortOffset.Index);
		ValuePort = (ushort)(BasePort + (uint)IOPortOffset.Value);

		WriteRegister(Register.ID, (uint)ID.V2);
		if (ReadRegister(Register.ID) != (uint)ID.V2)
		{
			throw new NotSupportedException("Un-supported SVGAII device! Please consider updating.");
		}

		FIFOMemory = new MemoryBlock(ReadRegister(Register.MemStart), ReadRegister(Register.MemSize));
		Features = ReadRegister(Register.Capabilities);
		InitializeFIFO();

		this.Height = Height;
		this.Width = Width;
	}

	#endregion

	#region Properties

	public override bool IsEnabled
	{
		get => ReadRegister(Register.Enable) == 1;
		set => WriteRegister(Register.Enable, value ? 1u : 0u);
	}

	public new ushort Height
	{
		get
		{
			return _Height;
		}
		set
		{
			// Memory resizing it already taken care of here.
			_Height = value;

			if (_Width != 0)
			{
				WriteRegister(Register.Width, Width);
				WriteRegister(Register.Height, Height);
				WriteRegister(Register.BitsPerPixel, 4);
				WriteRegister(Register.Enable, 1);
				InitializeFIFO();

				ScreenBuffer = (uint*)ReadRegister(Register.FrameBufferStart);
				Internal = ScreenBuffer + (Size * 4);
			}
		}
	}

	public new ushort Width
	{
		get
		{
			return _Width;
		}
		set
		{
			// Memory resizing it already taken care of here.
			_Width = value;

			if (_Height != 0)
			{
				WriteRegister(Register.Width, Width);
				WriteRegister(Register.Height, Height);
				WriteRegister(Register.BitsPerPixel, 4);
				WriteRegister(Register.Enable, 1);
				InitializeFIFO();

				ScreenBuffer = (uint*)ReadRegister(Register.FrameBufferStart);
				Internal = ScreenBuffer + (Size * 4);
			}
		}
	}

	#endregion

	#region Methods

	/// <summary>
	/// Write register.
	/// </summary>
	/// <param name="register">A register.</param>
	/// <param name="value">A value.</param>
	public void WriteRegister(Register register, uint value)
	{
		IOPort.Write32(IndexPort, (uint)register);
		IOPort.Write32(ValuePort, value);
	}

	/// <summary>
	/// Read register.
	/// </summary>
	/// <param name="register">A register.</param>
	/// <returns>uint value.</returns>
	public uint ReadRegister(Register register)
	{
		IOPort.Write32(IndexPort, (uint)register);
		return IOPort.Read32(ValuePort);
	}

	/// <summary>
	/// A method that checks if the device has a specific feature.
	/// </summary>
	/// <param name="Feature">The feature to check for.</param>
	/// <returns>True if supported, otherwise false.</returns>
	public bool HasFeature(Capability Feature)
	{
		return (Features & (uint)Feature) != 0;
	}

	/// <summary>
	/// Set FIFO.
	/// </summary>
	/// <param name="cmd">Command.</param>
	/// <param name="value">Value.</param>
	/// <returns></returns>
	public uint SetFIFO(FIFO cmd, uint value)
	{
		return FIFOMemory[(uint)cmd] = value;
	}

	/// <summary>
	/// Write to FIFO.
	/// </summary>
	/// <param name="value">Value to write.</param>
	public void WriteToFifo(uint value)
	{
		if ((GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max) - 4 && GetFIFO(FIFO.Stop) == GetFIFO(FIFO.Min)) || GetFIFO(FIFO.NextCmd) + 4 == GetFIFO(FIFO.Stop))
		{
			WaitForFifo();
		}

		SetFIFO((FIFO)GetFIFO(FIFO.NextCmd), value);
		SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.NextCmd) + 4);

		if (GetFIFO(FIFO.NextCmd) == GetFIFO(FIFO.Max))
		{
			SetFIFO(FIFO.NextCmd, GetFIFO(FIFO.Min));
		}
	}

	/// <summary>
	/// Get FIFO.
	/// </summary>
	/// <param name="cmd">FIFO command.</param>
	/// <returns>uint value.</returns>
	public uint GetFIFO(FIFO cmd)
	{
		return FIFOMemory[(uint)cmd];
	}

	/// <summary>
	/// Initialize FIFO.
	/// </summary>
	public void InitializeFIFO()
	{
		FIFOMemory[(uint)FIFO.Min] = (uint)Register.FifoNumRegisters * sizeof(uint);
		FIFOMemory[(uint)FIFO.Max] = FIFOMemory.Size;
		FIFOMemory[(uint)FIFO.NextCmd] = FIFOMemory[(uint)FIFO.Min];
		FIFOMemory[(uint)FIFO.Stop] = FIFOMemory[(uint)FIFO.Min];
		WriteRegister(Register.ConfigDone, 1);
	}

	/// <summary>
	/// Wait for FIFO.
	/// </summary>
	public void WaitForFifo()
	{
		WriteRegister(Register.Sync, 1);
		while (ReadRegister(Register.Busy) != 0) { }
	}

	public override void SetCursor(uint X, uint Y, bool IsVisible)
	{
		WriteRegister(Register.CursorOn, (uint)(IsVisible ? 1 : 0));
		WriteRegister(Register.CursorX, X);
		WriteRegister(Register.CursorY, Y);
		WriteRegister(Register.CursorCount, ReadRegister(Register.CursorCount) + 1);
	}

	public override void DefineCursor(Canvas Cursor)
	{
		if (!HasFeature(Capability.AlphaCursor))
		{
			throw new NotSupportedException("This device does not have accelerated cursor support.");
		}

		WaitForFifo();
		WriteToFifo((uint)FIFOCommand.DEFINE_ALPHA_CURSOR);
		WriteToFifo(0); // ID
		WriteToFifo(0); // Hotspot X
		WriteToFifo(0); // Hotspot Y
		WriteToFifo(Cursor.Width); // Width
		WriteToFifo(Cursor.Height); // Height

		for (uint I = 0; I < Cursor.Size; I++)
		{
			WriteToFifo(Cursor[I]);
		}

		WaitForFifo();
	}

	public override string GetName()
	{
		return nameof(SVGAIICanvas);
	}

	public override void Update()
	{
		CopyTo(ScreenBuffer);

		WriteToFifo((uint)FIFOCommand.Update);
		WriteToFifo(0);
		WriteToFifo(0);
		WriteToFifo(Width);
		WriteToFifo(Height);
		WaitForFifo();

		_Frames++;
	}

	#endregion

	#region Fields

	public readonly MemoryBlock FIFOMemory;
	public readonly PCIDevice Device;
	public readonly ushort IndexPort;
	public readonly ushort ValuePort;
	public readonly uint Features;
	public uint* ScreenBuffer;

	#endregion
}