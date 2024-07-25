using System.Numerics;

namespace PrismAPI.Graphics.Rasterizer;

/// <summary>
/// The <see cref="Engine"/> class - It is used to render 3D scenes onto a 2D canvas.
/// <list type="table">
/// <item>See also: <seealso cref="Triangle"/></item>
/// <item>See also: <seealso cref="Camera"/></item>
/// <item>See also: <seealso cref="Light"/></item>
/// <item>See also: <seealso cref="Mesh"/></item>
/// </list>
/// </summary>
public class Engine : Canvas
{
	#region Constructors

	/// <summary>
	/// Creates a new instance of the <see cref="Engine"/> class.
	/// </summary>
	/// <param name="Width">Width (in pixels) of the canvas.</param>
	/// <param name="Height">Height (in pixels) of the canvas.</param>
	/// <param name="FOV">The FOV (Field Of View) that the camera will have.</param>
	public Engine(ushort Width, ushort Height, float FOV) : base(Width, Height)
	{
		this.Height = Height;
		this.Width = Width;

		SkyColor = Color32.GoogleBlue;
		Lights = new();
		Objects = new();
		Camera = new(FOV);
		Zoom = 0f;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Renders the whole scene onto the internal canvas.
	/// You must draw this object as an image on another canvas to show the output.
	/// </summary>
	public void Render()
	{
		// Create the camera rotation matrix - This is separate from the object rotation.
		Quaternion CameraQ = Camera.GetRotationQuaternion();

		// Create a new projection value. This is equivalent to using a projection matrix, but it is easier.
		float Translator = (float)(Width / 2 / Math.Tan((Camera.FOV + Zoom) / 2 * 0.0174532925)); // 0.0174532925 == pi / 180

		// Set the sky color - Make sure to adjust for ambiant color aswell.
		Clear(Color32.Multiply(Color32.Normalize(SkyColor), Camera.Ambient));

		// Calculate Objects - Loops over all triangle in every mesh.
		foreach (Mesh M in Objects)
		{
			// Create a rotation matrix for the mesh - Separate from camera rotation.
			Matrix4x4 Rotation = M.GetRotationMatrix();

			foreach (Triangle T in M.Triangles)
			{
				// Create a temporary instance of the triangle that can be modified.
				Triangle Temp = T;

				Temp = Triangle.Transform(Temp, Rotation); // Apply object rotation - Separate from camera rotation.
				Temp = Triangle.Translate(Temp, M.Position); // Apply object translation - The position it is in 3D.
				Temp = Triangle.Transform(Temp, CameraQ); // Apply camera rotation - Rotates the entire world as one mesh.
				Temp = Triangle.Translate(Temp, Camera.Position); // Apply camera position - Adjusts the world as one mesh.
				Temp = Triangle.Translate(Temp, Translator); // Apply perspective translation - Applies a 3D effect.

				// Check if the triangle doesn't need to be drawn.
				if (Temp.GetNormal() < 0)
				{
					// Moves everything to center - Most 3D renderers do this.
					Temp = Triangle.Center(Temp, Width, Height);

					// Normalize lighting & apply ambiance.
					Temp.Color = Color32.Normalize(Temp.Color);
					Temp.Color = Color32.Multiply(Temp.Color, Camera.Ambient);

					// Rasterize the triangle.
					DrawFilledTriangle(Temp);
				}
			}
		}
	}

	#endregion

	#region Fields

	public List<Light> Lights;
	public List<Mesh> Objects;
	public uint SkyColor;
	public Camera Camera;
	public float Zoom;

	#endregion
}