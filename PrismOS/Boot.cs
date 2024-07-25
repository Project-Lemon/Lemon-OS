using PrismAPI.Graphics.Animation;
using PrismAPI.Hardware.GPU;
using PrismAPI.Graphics;

namespace PrismOS;

public static class Boot
{
	#region Constructors

	static Boot()
	{
		// Initialize the animations.
		A = new(25f, 270f, new(0, 0, 0, 0, 750), AnimationMode.Ease);
		B = new(0f, 360f, new(0, 0, 0, 0, 500), AnimationMode.Linear);
		C = new(Color32.Transparent, Color32.Black, new(0, 0, 0, 0, 500), AnimationMode.Ease);
		A.IsContinuous = true;
	}

	#endregion

	#region Methods

	private static void Update()
	{
		if (B.IsFinished)
		{
			B.Reset();
		}

		ushort H3 = (ushort)(Canvas.Height / 3);
		ushort W2 = (ushort)(Canvas.Width / 2);
		ushort H2 = (ushort)(Canvas.Height / 2);

		Canvas.Clear();
		Canvas.DrawImage(W2 - (H3 / 2), H2 - (H3 / 2), Media.Prism, false);

		int LengthOffset = (int)(B.Current + A.Current);
		int Offset = (int)B.Current;

		Canvas.DrawArc(W2, H2 + (H2 / 2) + (H3 / 2), 19, Color32.LightGray, Offset, LengthOffset);
		Canvas.DrawArc(W2, H2 + (H2 / 2) + (H3 / 2), 20, Color32.White, Offset, LengthOffset);
		Canvas.DrawArc(W2, H2 + (H2 / 2) + (H3 / 2), 21, Color32.LightGray, Offset, LengthOffset);
		Canvas.Update();
	}

	public static void Show(Display Canvas)
	{
		if (IsSetup)
		{
			IsEnabled = true;
			return;
		}

		// Assign the canvas instance.
		Boot.Canvas = Canvas;

		// Add a timer to update the screen while booting.
		IsEnabled = true;
		Timer T = new((_) => { if (IsEnabled) { Update(); }}, null, 40, 0);
		IsSetup = true;
	}

	public static void Hide()
	{
		IsEnabled = false;
		C.Reset();

		//while (!C.IsFinished)
		//{
		//	Update();
		//	Canvas.DrawFilledRectangle(0, 0, Canvas.Width, Canvas.Height, 0, C.Current);
		//	Canvas.Update();
		//}
	}

	#endregion

	#region Fields

	private static readonly AnimationController A;
	private static readonly AnimationController B;
	private static readonly ColorController C;
	private static Display Canvas = null!;
	private static bool IsEnabled;
	private static bool IsSetup;

	#endregion
}