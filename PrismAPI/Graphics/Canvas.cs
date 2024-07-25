using System.Runtime.InteropServices;
using PrismAPI.Graphics.Rasterizer;
using PrismAPI.Graphics.Fonts;
using System.Numerics;
using Cosmos.Core;

namespace PrismAPI.Graphics;

/// <summary>
/// The <see cref="Canvas"/> class, used for rendering content on a 2D surface.
///
/// <list type="table">
///		<item>See also: <see cref="Gradient"/></item>
///		<item>See also: <see cref="Filters"/></item>
///		<item>See also: <see cref="Image"/></item>
///		<item>See also: <see cref="Color"/></item>
/// </list>
/// </summary>
public unsafe class Canvas
{
	#region Constructors

	/// <summary>
	/// Creates a new instance of the <see cref="Canvas"/> class..
	/// </summary>
	/// <param name="Width">Width of the canvas.</param>
	/// <param name="Height">Height of the canvas.</param>
	public Canvas(ushort Width, ushort Height)
	{
		// Set the canvas size.
		this.Height = Height;
		this.Width = Width;
	}

	/// <summary>
	/// Destroys this instance of the <see cref="Canvas"/> class. Only used when it's finished being used.
	/// </summary>
	~Canvas()
	{
		if (Internal != null)
		{
			NativeMemory.Free(Internal);
		}
	}

	#endregion

	#region Properties

	/// <summary>
	/// Indexer to set or get a color value at the X and Y position.
	/// </summary>
	/// <param name="X">X position of the pixel.</param>
	/// <param name="Y">Y position of the pixel.</param>
	/// <returns>The pixel color at X and Y.</returns>
	public uint this[int X, int Y]
	{
		get
		{
			// Check if coordinates are out of bounds.
			if (X < 0 || Y < 0 || X >= Width || Y >= Height)
			{
				return Color32.Black;
			}

			return Internal[(Y * Width) + X];
		}
		set
		{
			// Check if coordinates are out of bounds.
			if (X < 0 || Y < 0 || X >= Width || Y >= Height)
			{
				return;
			}

			// Blend 2 colors together if the new color has alpha.
			if (Color32.GetAlpha(value) < 255)
			{
				value = Color32.AlphaBlend(this[X, Y], value);
			}

			Internal[(Y * Width) + X] = value;
		}
	}

	/// <summary>
	/// Indexer to set or get a color value at the specified index.
	/// </summary>
	/// <param name="Index">Index position of the pixel.</param>
	/// <returns>The pixel color at the linear index.</returns>
	public uint this[uint Index]
	{
		get
		{
			// Check if index is out of bounds.
			if (Index >= Size)
			{
				return Color32.Black;
			}

			return Internal[Index];
		}
		set
		{
			// Check if index is out of bounds.
			if (Index >= Size)
			{
				return;
			}

			// Blend 2 colors together if the new color has alpha.
			if (Color32.GetAlpha(value) < 255)
			{
				value = Color32.AlphaBlend(this[Index], value);
			}

			Internal[Index] = value;
		}
	}

	/// <summary>
	/// The total height of the canvas in pixels.
	/// </summary>
	public ushort Height
	{
		get => _Height;
		set
		{
			// Set new value.
			_Height = value;
			Size = (uint)(_Height * _Width);

			// Check if no allocations are needed.
			if (_Width == 0)
			{
				return;
			}

			if (Internal == null)
			{
				// Allocate new chunk of memory.
				Internal = (uint*)NativeMemory.Alloc(Size * 4);
			}
			else
			{
				// Re-allocate new memory & automatically free old chunk.
				GCImplementation.DecRootCount((ushort*)Internal);
				Internal = (uint*)NativeMemory.Realloc(Internal, Size * 4);
			}

			// Prevent GC from freeing the buffer.
			GCImplementation.IncRootCount((ushort*)Internal);
		}
	}

	/// <summary>
	/// The total width of the canvas in pixels.
	/// </summary>
	public ushort Width
	{
		get => _Width;
		set
		{
			// Set new value.
			_Width = value;
			Size = (uint)(_Height * _Width);

			// Check if no allocations are needed.
			if (_Height == 0)
			{
				return;
			}

			if (Internal == null)
			{
				// Allocate new chunk of memory.
				Internal = (uint*)NativeMemory.Alloc(Size * 4);
			}
			else
			{
				// Re-allocate new memory & automatically free old chunk.
				GCImplementation.DecRootCount((ushort*)Internal);
				Internal = (uint*)NativeMemory.Realloc(Internal, Size * 4);
			}

			// Prevent GC from freeing the buffer.
			GCImplementation.IncRootCount((ushort*)Internal);
		}
	}

	#endregion

	#region Methods

	#region Rectangle

	/// <summary>
	/// Draws a blurred rectangle from X and Y with the specified Width and Height.
	/// </summary>
	/// <param name="X">The X position to start at.</param>
	/// <param name="Y">The Y position to start at.</param>
	/// <param name="Width">Width of the rectangle.</param>
	/// <param name="Height">Height of the rectangle.</param>
	/// <param name="Radial">The blur intensity - 5 is typically fine.</param>
	public void DrawBlurredRectangle(int X, int Y, ushort Width, ushort Height, int Radial)
	{
		uint Size = (uint)(Width * Height);

		int[] A = new int[Size];
		int[] R = new int[Size];
		int[] G = new int[Size];
		int[] B = new int[Size];

		for (uint I = 0; I < Size; I++)
		{
			// Get the source X and Y position.
			int SX = (int)(X + (I % Width));
			int SY = (int)(Y + (I / Width));

			byte[] SplitColors = Color32.SplitChannels(this[SX, SY]);

			A[I] = SplitColors[0];
			R[I] = SplitColors[1];
			G[I] = SplitColors[2];
			B[I] = SplitColors[3];
		}

		var newAlpha = new int[Size];
		var newRed = new int[Size];
		var newGreen = new int[Size];
		var newBlue = new int[Size];

		Filters.GaussBlur4(A, newAlpha, Width, Height, Radial);
		Filters.GaussBlur4(R, newRed, Width, Height, Radial);
		Filters.GaussBlur4(G, newGreen, Width, Height, Radial);
		Filters.GaussBlur4(B, newBlue, Width, Height, Radial);

		for (uint I = 0; I < Size; I++)
		{
			newAlpha[I] = Math.Clamp(newAlpha[I], 0, 255);
			newRed[I] = Math.Clamp(newRed[I], 0, 255);
			newGreen[I] = Math.Clamp(newGreen[I], 0, 255);
			newBlue[I] = Math.Clamp(newBlue[I], 0, 255);

			// Get the source X and Y position.
			int SX = (int)(X + (I % Width));
			int SY = (int)(Y + (I / Width));

			this[SX, SY] = Color32.FromARGB(newAlpha[I], newRed[I], newGreen[I], newBlue[I]);
		}
	}

	/// <summary>
	/// Draws a filled rectangle from X and Y with the specified Width and Height.
	/// </summary>
	/// <param name="X">The X position to start at.</param>
	/// <param name="Y">The Y position to start at.</param>
	/// <param name="Width">Width of the rectangle.</param>
	/// <param name="Height">Height of the rectangle.</param>
	/// <param name="Radius">The border radius of the rectangle.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	public void DrawFilledRectangle(int X, int Y, ushort Width, ushort Height, ushort Radius, uint Color)
	{
		// Quit if nothing needs to be drawn.
		if (X >= this.Width || Y >= this.Height)
		{
			return;
		}
		if (X + Width < 0 || Y + Height < 0)
		{
			return;
		}

		// Fastest cropped draw method.
		if (Color32.GetAlpha(Color) == 255 && Radius == 0)
		{
			// Fastest copy-only draw method, fills the whole buffer.
			if (X == 0 && Y == 0 && Width == this.Width && Height == this.Height)
			{
				Clear(Color);
				return;
			}

			// Get the cropped coordinates.
			uint StartX = (uint)Math.Max(X, 0);
			uint StartY = (uint)Math.Max(Y, 0);
			uint EndX = (uint)Math.Min(X + Width, this.Width);
			uint EndY = (uint)Math.Min(Y + Height, this.Height);

			// Get new size after crop.
			uint RHeight = EndY - StartY;
			uint RWidth = EndX - StartX;

			// Calculate destination offset for the starting point
			uint Destination = (StartY * this.Width) + StartX;

			// Fill the region with the color
			for (uint IY = 0; IY < RHeight; IY++)
			{
				MemoryOperations.Fill(Internal + Destination + (IY * this.Width), Color, (int)RWidth);
			}
			return;
		}

		// Fastest alpha supporting rectangle.
		if (Radius == 0)
		{
			for (int I = 0; I < Width * Height; I++)
			{
				int IX = I % Width;
				int IY = I / Width;

				this[X + IX, Y + IY] = Color;
			}
		}

		// Circular rectangle.
		else
		{
			if (Height == Radius * 2)
			{
				DrawFilledCircle(X + Radius, Y + Radius, Radius, Color);
				DrawFilledCircle(X + Width + Radius, Y + Radius, Radius, Color);
				DrawFilledRectangle(X + Radius, Y, Width, Height, 0, Color);
				return;
			}
			if (Width == Radius * 2)
			{
				DrawFilledCircle(X + Radius, Y + Radius, Radius, Color);
				DrawFilledCircle(X + Width + Radius, Y + Radius, Radius, Color);
				DrawFilledRectangle(X, Y + Radius, Width, Height, 0, Color);
				return;
			}

			DrawFilledCircle(X + Radius, Y + Radius, Radius, Color);
			DrawFilledCircle(X + Width - Radius - 1, Y + Radius, Radius, Color);

			DrawFilledCircle(X + Radius, Y + Height - Radius - 1, Radius, Color);
			DrawFilledCircle(X + Width - Radius - 1, Y + Height - Radius - 1, Radius, Color);

			DrawFilledRectangle(X + Radius, Y, (ushort)(Width - (Radius * 2)), Height, 0, Color);
			DrawFilledRectangle(X, Y + Radius, Width, (ushort)(Height - (Radius * 2)), 0, Color);
		}
	}

	/// <summary>
	/// Draws a non-filled rectangle from X and Y with the specified Width and Height.
	/// </summary>
	/// <param name="X">The X position to start at.</param>
	/// <param name="Y">The Y position to start at.</param>
	/// <param name="Width">Width of the rectangle.</param>
	/// <param name="Height">Height of the rectangle.</param>
	/// <param name="Radius">The border radius of the rectangle.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	public void DrawRectangle(int X, int Y, ushort Width, ushort Height, ushort Radius, uint Color)
	{
		// Draw circles to add curvature if needed.
		if (Radius > 0)
		{
			DrawArc(Radius + X, Radius + Y, Radius, Color, 180, 270); // Top left
			DrawArc(X + Width - Radius, Y + Height - Radius, Radius, Color, 0, 90); // Bottom right
			DrawArc(Radius + X, Y + Height - Radius, Radius, Color, 90, 180); // Bottom left
			DrawArc(X + Width - Radius, Radius + Y, Radius, Color, 270, 360);
		}

		DrawLine(X + Radius, Y, X + Width - Radius, Y, Color); // Top Line
		DrawLine(X + Radius, Y + Height, X + Width - Radius, Height + Y, Color); // Bottom Line
		DrawLine(X, Y + Radius, X, Y + Height - Radius, Color); // Left Line
		DrawLine(X + Width, Y + Radius, Width + X, Y + Height - Radius, Color); // Right Line
	}

	/// <summary>
	/// Draws a grid of mixed blocks where each block has a size and count, creates a pattern.
	/// </summary>
	/// <param name="X">The X position to start at.</param>
	/// <param name="Y">The Y position to start at.</param>
	/// <param name="BlockCountX">Number of blocks in the X axis.</param>
	/// <param name="BlockCountY">Number of blocks in the Y axis.</param>
	/// <param name="BlockSize">Scale of all blocks.</param>
	/// <param name="BlockType1">Color of block type 1.</param>
	/// <param name="BlockType2">Color of block type 2.</param>
	public void DrawRectangleGrid(int X, int Y, ushort BlockCountX, ushort BlockCountY, ushort BlockSize, uint BlockType1, uint BlockType2)
	{
		for (int IX = 0; IX < BlockCountX; IX++)
		{
			for (int IY = 0; IY < BlockCountY; IY++)
			{
				if ((IX + IY) % 2 == 0)
				{
					DrawFilledRectangle(X + (IX * BlockSize), Y + (IY * BlockSize), BlockSize, BlockSize, 0, BlockType1);
				}
				else
				{
					DrawFilledRectangle(X + (IX * BlockSize), Y + (IY * BlockSize), BlockSize, BlockSize, 0, BlockType2);
				}
			}
		}
	}

	#endregion

	#region Triangle

	/// <summary>
	/// Draws a filled triangle as marked by the triangle class.
	/// </summary>
	/// <param name="Triangle">The triangle coordinates.</param>
	public void DrawFilledTriangle(Triangle Triangle)
	{
		Triangle.P1 *= 16;
		Triangle.P2 *= 16;
		Triangle.P3 *= 16;

		// Deltas
		Vector3 D12 = Triangle.P1 - Triangle.P2;
		Vector3 D23 = Triangle.P2 - Triangle.P3;
		Vector3 D31 = Triangle.P3 - Triangle.P1;

		// Fixed-point deltas
		int FDX12 = (int)D12.X << 4;
		int FDX23 = (int)D23.X << 4;
		int FDX31 = (int)D31.X << 4;

		int FDY12 = (int)D12.Y << 4;
		int FDY23 = (int)D23.Y << 4;
		int FDY31 = (int)D31.Y << 4;

		// Bounding rectangle
		Vector3 Min = Vector3.Min(Vector3.Min(Triangle.P1, Triangle.P2), Triangle.P3);
		Vector3 Max = Vector3.Max(Vector3.Max(Triangle.P1, Triangle.P2), Triangle.P3);

		// Some math things - Idk what they do but they are needed.
		Min.X = (int)(Min.X + 0xF) >> 4;
		Min.Y = (int)(Min.Y + 0xF) >> 4;
		Min.Z = (int)(Min.Z + 0xF) >> 4;
		Max.X = (int)(Max.X + 0xF) >> 4;
		Max.Y = (int)(Max.Y + 0xF) >> 4;
		Max.Z = (int)(Max.Z + 0xF) >> 4;

		// Half-edge constants
		int C1 = (int)((D12.Y * Triangle.P1.X) - (D12.X * Triangle.P1.Y));
		int C2 = (int)((D23.Y * Triangle.P2.X) - (D23.X * Triangle.P2.Y));
		int C3 = (int)((D31.Y * Triangle.P3.X) - (D31.X * Triangle.P3.Y));

		// Correct for fill convention
		if (D12.Y < 0 || (D12.Y == 0 && D12.X > 0)) C1++;
		if (D23.Y < 0 || (D23.Y == 0 && D23.X > 0)) C2++;
		if (D31.Y < 0 || (D31.Y == 0 && D31.X > 0)) C3++;

		int CY1 = (int)(C1 + (D12.X * ((int)Min.Y << 4)) - (D12.Y * ((int)Min.X << 4)));
		int CY2 = (int)(C2 + (D23.X * ((int)Min.Y << 4)) - (D23.Y * ((int)Min.X << 4)));
		int CY3 = (int)(C3 + (D31.X * ((int)Min.Y << 4)) - (D31.Y * ((int)Min.X << 4)));

		for (int Y = (int)Min.Y; Y < Max.Y; Y++)
		{
			// Don't draw outside of the screen.
			if (Y >= Height || Y < 0)
			{
				continue;
			}

			int CX1 = CY1;
			int CX2 = CY2;
			int CX3 = CY3;

			for (int X = (int)Min.X; X < Max.X; X++)
			{
				// Don't draw outside of the screen.
				if (X >= Width || X < 0)
				{
					continue;
				}

				if (CX1 > 0 && CX2 > 0 && CX3 > 0)
				{
					this[X, Y] = Triangle.Color;
				}

				CX1 -= FDY12;
				CX2 -= FDY23;
				CX3 -= FDY31;
			}

			CY1 += FDX12;
			CY2 += FDX23;
			CY3 += FDX31;
		}
	}

	/// <summary>
	/// Draws a non-filled triangle as marked by the triangle class.
	/// </summary>
	/// <param name="Triangle">The triangle coordinates.</param>
	public void DrawTriangle(Triangle Triangle)
	{
		DrawLine((int)Triangle.P1.X, (int)Triangle.P1.Y, (int)Triangle.P2.X, (int)Triangle.P2.Y, Triangle.Color);
		DrawLine((int)Triangle.P1.X, (int)Triangle.P1.X, (int)Triangle.P3.X, (int)Triangle.P3.Y, Triangle.Color);
		DrawLine((int)Triangle.P2.X, (int)Triangle.P2.Y, (int)Triangle.P3.X, (int)Triangle.P3.Y, Triangle.Color);
	}

	#endregion

	#region Circle 

	/// <summary>
	/// Draws a filled circle where X and Y are the center of it.
	/// </summary>
	/// <param name="X">Center X of the circle.</param>
	/// <param name="Y">Center Y of the circle.</param>
	/// <param name="Radius">Radius of the circle.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	public void DrawFilledCircle(int X, int Y, ushort Radius, uint Color)
	{
		// Quit if there is nothing to draw.
		if (Radius == 0)
		{
			return;
		}

		// Check if the circle can be drawn fast.
		if (Color32.GetAlpha(Color) == 255)
		{
			ushort R2 = (ushort)(Radius * Radius);

			// Loop for each line in the circle.
			for (int IY = -Radius; IY <= Radius; IY++)
			{
				int IX = (int)(Math.Sqrt(R2 - (IY * IY)) + 0.5);
				uint* Offset = Internal + (Width * (Y + IY)) + X - IX;

				// Clip circle if it is out of bounds
				if (X + Radius >= Width)
				{
					// Reduce length to fit to max width.
					IX -= X + Radius - Width;
				}
				if (X - Radius < 0)
				{
					// Reduce length and offset so that it stays at X = 0
					Offset += Radius - +X;
					IX -= Radius - +X;
				}

				// Fill one line of pixels.
				MemoryOperations.Fill(Offset, Color, IX * 2);
			}

			// Be sure to return.
			return;
		}

		// Draw using slow algorithm.
		for (int IX = -Radius; IX < Radius; IX++)
		{
			int Height = (int)Math.Sqrt((Radius * Radius) - (IX * IX));

			for (int IY = -Height; IY < Height; IY++)
			{
				this[IX + X, IY + Y] = Color;
			}
		}
	}

	/// <summary>
	/// Draws a non-filled circle where X and Y are the center of it.
	/// </summary>
	/// <param name="X">Center X of the circle.</param>
	/// <param name="Y">Center Y of the circle.</param>
	/// <param name="Radius">Radius of the circle.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	public void DrawCircle(int X, int Y, ushort Radius, uint Color)
	{
		int IX = 0, IY = Radius, DP = 3 - (2 * Radius);

		while (IY >= IX)
		{
			this[X + IX, Y + IY] = Color;
			this[X - IX, Y + IY] = Color;
			this[X + IX, Y - IY] = Color;
			this[X - IX, Y - IY] = Color;
			this[X + IY, Y + IX] = Color;
			this[X - IY, Y + IX] = Color;
			this[X + IY, Y - IX] = Color;
			this[X - IY, Y - IX] = Color;

			IX++;

			if (DP > 0)
			{
				IY--;
				DP += (4 * (IX - IY)) + 10;
			}
			else
			{
				DP += (4 * IX) + 6;
			}
		}
	}

	#endregion

	#region Lines

	/// <summary>
	/// Draws a quadratic bezier curve from point A (X1, Y1) to point B (X3, Y3)
	/// </summary>
	/// <param name="X1">X position 1.</param>
	/// <param name="Y1">Y position 1.</param>
	/// <param name="X2">X weight.</param>
	/// <param name="Y2">Y weight.</param>
	/// <param name="X3">X position 2.</param>
	/// <param name="Y3">Y position 2.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	/// <param name="N">Used inside the method only.</param>
	public void DrawQuadraticBezierLine(int X1, int Y1, int X2, int Y2, int X3, int Y3, uint Color, byte N = 6)
	{
		// X2 and Y2 is where the curve should bend to.
		if (N > 0)
		{
			int X12 = (X1 + X2) / 2;
			int Y12 = (Y1 + Y2) / 2;
			int X23 = (X2 + X3) / 2;
			int Y23 = (Y2 + Y3) / 2;
			int X123 = (X12 + X23) / 2;
			int Y123 = (Y12 + Y23) / 2;

			DrawQuadraticBezierLine(X1, Y1, X12, Y12, X123, Y123, Color, (byte)(N - 1));
			DrawQuadraticBezierLine(X123, Y123, X23, Y23, X3, Y3, Color, (byte)(N - 1));
		}
		else
		{
			DrawLine(X1, Y1, X2, Y2, Color);
			DrawLine(X2, Y2, X3, Y3, Color);
		}
	}

	/// <summary>
	/// Draws a cubic bezier curve from point A (X1, Y1) to point B (X4, Y4)
	/// </summary>
	/// <param name="X1">X position 1.</param>
	/// <param name="Y1">Y position 1.</param>
	/// <param name="X2">X weight 1.</param>
	/// <param name="Y2">Y weight 1.</param>
	/// <param name="X3">X weight 2.</param>
	/// <param name="Y3">Y weight 2.</param>
	/// <param name="X4">X position 2.</param>
	/// <param name="Y4">Y position 2.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	public void DrawCubicBezierLine(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, uint Color)
	{
		for (double U = 0.0; U <= 1.0; U += 0.0001)
		{
			double Power3V1 = (1 - U) * (1 - U) * (1 - U);
			double Power2V1 = (1 - U) * (1 - U);
			double Power3V2 = U * U * U;
			double Power2V2 = U * U;

			double XU = (Power3V1 * X1) + (3 * U * Power2V1 * X2) + (3 * Power2V2 * (1 - U) * X3) + (Power3V2 * X4);
			double YU = (Power3V1 * Y1) + (3 * U * Power2V1 * Y2) + (3 * Power2V2 * (1 - U) * Y3) + (Power3V2 * Y4);

			this[(int)XU, (int)YU] = Color;
		}
	}

	/// <summary>
	/// Draws a line at an angle from X and Y with a circle's radius.
	/// </summary>
	/// <param name="X">X position.</param>
	/// <param name="Y">Y position.</param>
	/// <param name="Angle">Angle in degrees.</param>
	/// <param name="Radius">Radius or Length.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	public void DrawAngledLine(int X, int Y, short Angle, ushort Radius, uint Color)
	{
		int IX = (int)(Radius * Math.Cos(Math.PI * Angle / 180));
		int IY = (int)(Radius * Math.Sin(Math.PI * Angle / 180));

		DrawLine(X, Y, X + IX, Y + IY, Color);
	}

	/// <summary>
	/// Draws a line from point A (X1, Y1) to point B (X2, Y2)
	/// </summary>
	/// <param name="X1">X position 1.</param>
	/// <param name="Y1">Y position 1.</param>
	/// <param name="X2">X positoin 2.</param>
	/// <param name="Y2">Y position 2.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	public void DrawLine(int X1, int Y1, int X2, int Y2, uint Color)
	{
		int DX = Math.Abs(X2 - X1), SX = X1 < X2 ? 1 : -1;
		int DY = Math.Abs(Y2 - Y1), SY = Y1 < Y2 ? 1 : -1;
		int err = (DX > DY ? DX : -DY) / 2;

		while (X1 != X2 || Y1 != Y2)
		{
			this[X1, Y1] = Color;

			int E2 = err;

			if (E2 > -DX) { err -= DY; X1 += SX; }
			if (E2 < DY) { err += DX; Y1 += SY; }
		}
	}

	#endregion

	#region Arc

	/// <summary>
	/// Draws a non-filled arc at the center of X and Y with a Width and a Height.
	/// </summary>
	/// <param name="X">X position.</param>
	/// <param name="Y">Y position.</param>
	/// <param name="Width">Width of the arc.</param>
	/// <param name="Height">Height of the arc.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	/// <param name="StartAngle">Angle at which to start.</param>
	/// <param name="EndAngle">Angle at which to end.</param>
	public void DrawArc(int X, int Y, ushort Width, ushort Height, uint Color, int StartAngle = 0, int EndAngle = 360)
	{
		// Quit if nothing needs to be drawn.
		if (Width == 0 || Height == 0)
		{
			return;
		}

		for (double Angle = StartAngle; Angle < EndAngle; Angle += 0.5)
		{
			double Angle1 = Math.PI * Angle / 180;

			int IX = (int)Math.Clamp(Width * Math.Cos(Angle1), -Width + 1, Width - 1);
			int IY = (int)Math.Clamp(Height * Math.Sin(Angle1), -Height + 1, Height - 1);

			this[X + IX, Y + IY] = Color;
		}
	}

	/// <summary>
	/// Draws a non-filled arc at the center of X and Y with a radius.
	/// </summary>
	/// <param name="X">X position.</param>
	/// <param name="Y">Y position.</param>
	/// <param name="Radius">Radius of the arc.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	/// <param name="StartAngle">Angle at which to start.</param>
	/// <param name="EndAngle">Angle at which to end.</param>
	public void DrawArc(int X, int Y, ushort Radius, uint Color, int StartAngle = 0, int EndAngle = 360)
	{
		// Quit if nothing needs to be drawn.
		if (Radius == 0)
		{
			return;
		}

		DrawArc(X, Y, Radius, Radius, Color, StartAngle, EndAngle);
	}

	#endregion

	#region Image

	/// <summary>
	/// Draws an image and X and Y.
	/// </summary>
	/// <param name="X">X position.</param>
	/// <param name="Y">Y position.</param>
	/// <param name="Image">Image to draw.</param>
	/// <param name="Alpha">Option to not use alpha, it will be faster if it's disabled.</param>
	/// <exception cref="NullReferenceException">Thrown when input is null.</exception>
	public void DrawImage(int X, int Y, Canvas Image, bool Alpha = true)
	{
		// Basic null/empty check.
		if (Image == null || Image.Width == 0 || Image.Height == 0)
		{
			return;
		}

		// Quit if nothing needs to be drawn.
		if (X + Image.Width < 0 || Y + Image.Height < 0 || X >= Width || Y >= Height)
		{
			return;
		}

		// Fastest cropped draw method.
		if (!Alpha)
		{
			// Fastest copy-only draw method, fills the whole buffer.
			if (X == 0 && Y == 0 && Image.Width == this.Width && Image.Height == this.Height)
			{
				Buffer.MemoryCopy(Image.Internal, Internal, Size * 4, Size * 4);
				return;
			}

			// Get the cropped coordinates.
			uint StartX = (uint)Math.Max(X, 0);
			uint StartY = (uint)Math.Max(Y, 0);
			uint EndX = (uint)Math.Min(X + Image.Width, this.Width);
			uint EndY = (uint)Math.Min(Y + Image.Height, this.Height);

			// Get new size after crop.
			uint Height = EndY - StartY;
			uint Width = EndX - StartX;

			// Calculate destination & source offsets.
			uint Destination = (StartY * this.Width) + StartX;
			uint Source = (uint)(((StartY - Y) * Image.Width) + (StartX - X));

			// Draw each line.
			for (uint IY = 0; IY < Height; IY++)
			{
				Buffer.MemoryCopy(Image.Internal + Source, Internal + Destination, Width * 4, Width * 4);

				// Increment the offsets.
				Destination += this.Width;
				Source += Image.Width;
			}
			return;
		}

		// Last resort for alpha images.
		for (int I = 0; I < Image.Width * Image.Height; I++)
		{
			int IX = I % Image.Width;
			int IY = I / Image.Width;

			this[X + IX, Y + IY] = Image[IX, IY];
		}
	}

	#endregion

	#region Text

	/// <summary>
	/// Draws a string of text at X and Y.
	/// </summary>
	/// <param name="X">X position.</param>
	/// <param name="Y">Y position.</param>
	/// <param name="Text">Text to draw.</param>
	/// <param name="Font">Font to use.</param>
	/// <param name="Color">The <see cref="Color"/> object to draw with.</param>
	/// <param name="Center">Option to cented the text at X and Y.</param>
	public void DrawString(int X, int Y, string Text, Font? Font, uint Color, bool Center = false)
	{
		// Basic null check.
		if (string.IsNullOrEmpty(Text))
		{
			return;
		}

		// Quit if nothing needs to be drawn.
		if (X >= Width || Y >= Height)
		{
			return;
		}

		// Allow the use of the 'default' keyword for text drawing.
		if (Font == default)
		{
			Font = Font.Fallback;
		}

		// Pre-calculate the string's size.
		ushort TextWidth = Font.MeasureString(Text);

		// Create temporary coordinates.
		int BX = X;
		int BY = Y;

		// Check if the text needs to be centered.
		if (Center)
		{
			int NewlineCount = Text.Count(C => C == '\n');
			BY -= Font.Size * (NewlineCount + 1) / 2;
			BX -= TextWidth / 2;
		}

		// Loop Through Each Line Of Text
		for (int I = 0; I < Text.Length; I++)
		{
			switch (Text[I])
			{
				case '\n':
					BX = X - (Center ? TextWidth / 2 : 0);
					BY += Font.Size;
					continue;
				case '\0':
					continue;
				case ' ':
					BX += Font.Size / 2;
					continue;
				case '\t':
					BX += Font.Size * 4;
					continue;
			}

			// Get the glyph for this char.
			Glyph Temp = Font.GetGlyph(Text[I]);

			// Draw all pixels.
			for (int P = 0; P < Temp.Points.Count; P++)
			{
				this[BX + Temp.Points[P].X, BY + Temp.Points[P].Y] = Color;
			}

			// Offset the X position by the glyph's length.
			BX += Temp.Width + 2;
		}
	}

	#endregion

	#region Misc

	/// <summary>
	/// Clears the canvas with the specified color.
	/// </summary>
	/// <param name="Color">Color to clear the canvas with.</param>
	public void Clear(uint Color)
	{
		MemoryOperations.Fill(Internal, Color, (int)Size);
	}

	/// <summary>
	/// Clears the canvas.
	/// </summary>
	public void Clear()
	{
		Clear(Color32.Black);
	}

	/// <summary>
	/// Copies the raw pixel data to an address in memory.
	/// </summary>
	/// <param name="Destination">Desination address to copy to.</param>
	public void CopyTo(uint* Destination)
	{
		Buffer.MemoryCopy(Internal, Destination, Size * 4, Size * 4);
	}

	/// <summary>
	/// Performs a quick display swap between the two canvases.
	/// Cannot be used on raw device buffers.
	/// This should be used when display updates run on their own thread, and a screen element
	/// has a separate clock cycle.
	/// </summary>
	/// <param name="Canvas">The canvas to swap.</param>
	public void Swap(Canvas Canvas)
	{
		if (Canvas._Width != _Width || Canvas._Height != Height)
		{
			throw new Exception("Attempted to swap two different sized buffers.");
		}
		
		// This simply swaps the addresses, leading to an instant "transfer".
		uint* Source = Canvas.Internal;
		Canvas.Internal = Internal;
		Internal = Source;
	}

	#endregion

	#endregion

	#region Fields

	/// <summary>
	/// The internal Height value cache.
	/// </summary>
	internal ushort _Height;

	/// <summary>
	/// The internal Width value cache.
	/// </summary>
	internal ushort _Width;

	/// <summary>
	/// The graphics frame buffer, with a size matching <see cref="Size"/>.
	/// </summary>
	internal uint* Internal;

	/// <summary>
	/// This represents the size of the canvas. It should not be changed directly.
	/// </summary>
	internal uint Size;

	#endregion
}