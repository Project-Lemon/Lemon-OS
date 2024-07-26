using PrismAPI.Runtime.SystemCall;
using PrismAPI.Tools.Extentions;
using PrismAPI.Hardware.GPU;
using PrismAPI.Filesystem;
using Cosmos.Core.Memory;
using PrismAPI.Graphics;
using PrismAPI.Network;
using PrismAPI.Audio;
using Cosmos.System;
using Cosmos.Core;

namespace PrismOS;

/*
// TO-DO: raycaster engine.
// TO-DO: Fix gradient's MaskAlpha method. (?)
// TO-DO: Move 3D engine to be shader based for all transformations.
// TO-DO: Convert canvas to a static class and reference simple objects instead.
*/
public class Program : Kernel
{
	#region Methods

	/// <summary>
	/// A method called once when the kernel boots, Used to initialize the system.
	/// </summary>
	protected override void BeforeRun()
	{
		// Initialize the display output.
		Canvas = Display.GetDisplay(1280, 640);

		// Scale the boot slash and wallpaper images.
		Media.Prism = Filters.Scale((ushort)(Canvas.Height / 3), (ushort)(Canvas.Height / 3), Media.Prism);
		Boot.Show(Canvas);
		Media.Wallpaper = Filters.Scale(Canvas.Width, Canvas.Height, Media.Wallpaper);

		// Initialize system services.
		FilesystemManager.Init();
		NetworkManager.Init();
		AudioPlayer.Init();
		Handler.Init();

		// Disable the screen timer.
		Boot.Hide();

		AudioPlayer.Play(Media.Startup);
	}

	/// <summary>
	/// A method called repeatedly until the kernel stops.
	/// </summary>
	protected override void Run()
	{
		// Draw the wallpaper.
		Canvas.DrawImage(0, 0, Media.Wallpaper, false);

		// Example of a drawable widget.
		Canvas.DrawString(128, 16, $"{Canvas.GetFPS()} FPS", default, Color32.White);

		// Draw the mouse on screen, then update.
		Canvas.DrawImage((int)MouseManager.X, (int)MouseManager.Y, Media.Cursor);
		Canvas.Update();

		// Clean memory every 30 frames.
		if (FrameCount++ >= 30)
		{
			Heap.Collect();
		}
	}

	#endregion

	#region Fields

	public static Display Canvas = null!;
	private static int FrameCount = 0;

	#endregion
}