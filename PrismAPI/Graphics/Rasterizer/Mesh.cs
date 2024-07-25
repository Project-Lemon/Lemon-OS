using System.Numerics;

namespace PrismAPI.Graphics.Rasterizer;

/// <summary>
/// The <see cref="Mesh"/> class - Used to generate and load complet 3D shapes. Used in the <see cref="Engine"/> class.
/// <list type="table">
/// <item>See also: <seealso cref="Triangle"/></item>
/// <item>See also: <seealso cref="Vector3"/></item>
/// </list>
/// </summary>
public class Mesh
{
	#region Constructors

	/// <summary>
	/// Creates a new instance of the <see cref="Mesh"/> class.
	/// </summary>
	public Mesh()
	{
		Triangles = new();
		Position = Vector3.Zero;
		Rotation = Vector3.Zero;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Creates a new <see cref="Mesh"/> object from a '.obj' 3D file.
	/// See: https://docs.fileformat.com/3d/obj/
	/// </summary>
	/// <param name="Object">The binary data of the object.</param>
	/// <returns>A mesh containing the object.</returns>
	public static Mesh FromObject(byte[] Object, float Scale = 1f)
	{
		List<Vector3> Vertices = new(); // Create verticies buffer.
		Mesh Temp = new(); // Create temporary mesh object.

		// Create stream reader to read the input file.
		StreamReader Reader = new(new MemoryStream(Object));

		// Keep reading until the end of the file is reached.
		while (Reader.BaseStream.Position < Reader.BaseStream.Length)
		{
			// Get next line from input.
			string? Line = Reader.ReadLine();

			// Skip blank lines and comments
			if (string.IsNullOrWhiteSpace(Line) || Line[0] == '#')
			{
				continue;
			}

			// Trim & split input into all correct components.
			string[] Components = Line.Trim().Split(' ');

			// Check if line is empty - If so, skip it.
			if (Components.Length == 0)
			{
				continue;
			}

			// Check what kind of entry the line is.
			switch (Components[0])
			{
				// Vertex - One point in a triangle.
				case "v":
					// Throw error when an invalid input is detected.
					if (Components.Length != 4)
					{
						throw new FormatException("Invalid vertex format.");
					}

					// Attempt to load all coordinates for the vector into the vertex cache.
					try
					{
						Vertices.Add(new()
						{
							X = float.Parse(Components[1]) * Scale,
							Y = float.Parse(Components[2]) * Scale,
							Z = float.Parse(Components[3]) * Scale,
						});
					}
					catch
					{
						// Invalid vertex format!
						continue;
					}
					break;

				// Face - One triangle in the mesh.
				case "F":
					// Throw error when an invalid input is detected.
					if (Components.Length != 4)
					{
						throw new FormatException("Invalid face format.");
					}

					string[] ComponentsV1 = Components[1].Split('/');
					string[] ComponentsV2 = Components[2].Split('/');
					string[] ComponentsV3 = Components[3].Split('/');

					// Throw error when an invalid input is detected.
					if (ComponentsV1.Length != 1 || ComponentsV2.Length != 1 || ComponentsV3.Length != 1)
					{
						// Invalid vertex format!
						continue;
					}

					// Attempt to load proper vertices from the vector cache as a triangle into the mesh.
					try
					{
						Temp.Triangles.Add(new()
						{
							P1 = Vertices[int.Parse(ComponentsV1[0]) - 1],
							P2 = Vertices[int.Parse(ComponentsV2[0]) - 1],
							P3 = Vertices[int.Parse(ComponentsV3[0]) - 1],
						});
					}
					catch
					{
						// Invalid vertex format!
						continue;
					}
					break;
			}
		}

		// Return the temporary mesh value.
		return Temp;
	}

	/// <summary>
	/// Creates a new <see cref="Mesh"/> object that is a 'Cube', with a specifc size.
	/// </summary>
	/// <param name="Width">The Width of the cube.</param>
	/// <param name="Height">The Height of the cube.</param>
	/// <param name="Length">The Length of the cube.</param>
	/// <returns>A mesh object that is in the form of a cube, whos size is determined by the input.</returns>
	public static Mesh GetCube(float Width, float Height, float Length)
	{
		return new()
		{
			Triangles = new()
			{
					// South
					new(-(Width / 2), -(Height / 2), -(Length / 2), -(Width / 2), Height / 2, -(Length / 2), Width / 2, Height / 2, -(Length / 2), Color32.White),
				new(-(Width / 2), -(Height / 2), -(Length / 2), Width / 2, Height / 2, -(Length / 2), Width / 2, -(Height / 2), -(Length / 2), Color32.White),

					// East
					new(Width / 2, -(Height / 2), -(Length / 2), Width / 2, Height / 2, -(Length / 2), Width / 2, Height / 2, Length / 2, Color32.CoolGreen),
				new(Width / 2, -(Height / 2), -(Length / 2), Width / 2, Height / 2, Length / 2, Width / 2, -(Height / 2), Length / 2, Color32.CoolGreen),

					// North
					new(Width / 2, -(Height / 2), Length / 2, Width / 2, Height / 2, Length / 2, -(Width / 2), Height / 2, Length / 2, Color32.White),
				new(Width / 2, -(Height / 2), Length / 2, -(Width / 2), Height / 2, Length / 2, -(Width / 2), -(Height / 2), Length / 2, Color32.White),

					// West
					new(-(Width / 2), -(Height / 2), Length / 2, -(Width / 2), Height / 2, Length / 2, -(Width / 2), Height / 2, -(Length / 2), Color32.CoolGreen),
				new(-(Width / 2), -(Height / 2), Length / 2, -(Width / 2), Height / 2, -(Length / 2), -(Width / 2), -(Height / 2), -(Length / 2), Color32.CoolGreen),

					// Top
					new(-(Width / 2), Height / 2, -(Length / 2), -(Width / 2), Height / 2, Length / 2, Width / 2, Height / 2, Length / 2, Color32.White),
				new(-(Width / 2), Height / 2, -(Length / 2), Width / 2, Height / 2, Length / 2, Width / 2, Height / 2, -(Length / 2), Color32.White),

					// Bottom
					new(Width / 2, -(Height / 2), Length / 2, -(Width / 2), -(Height / 2), Length / 2, -(Width / 2), -(Height / 2), -(Length / 2), Color32.White),
				new(Width / 2, -(Height / 2), Length / 2, -(Width / 2), -(Height / 2), -(Length / 2), Width / 2, -(Height / 2), -(Length / 2), Color32.White),
			},
		};
	}

	/// <summary>
	/// Creates a new <see cref="Mesh"/> object that is a 'Plane', a specific with and height and 1 pixel tall.
	/// </summary>
	/// <param name="Width">The Width of the plane.</param>
	/// <param name="Length">The Height of the plane.</param>
	/// <returns>A new mesh plane, whos size is determined by the input.</returns>
	public static Mesh GetPlane(float Width, float Length)
	{
		return GetCube(Width, 1, Length);
	}

	/// <summary>
	/// Gets the rotation matrix for the mesh.
	/// <list type="table">
	/// <item>See: <see cref="Engine.Render"/></item>
	/// </list>
	/// </summary>
	/// <returns>The rotation matrix based on the mesh's rotation.</returns>
	public Matrix4x4 GetRotationMatrix()
	{
		return Matrix4x4.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);
	}

	public void TestLogic(float ElapsedTime)
	{
		//Rotation.Y += (DateTime.Now - LTime).TotalSeconds;
		//LTime = DateTime.Now;
		Rotation.X += ElapsedTime;
	}

	#endregion

	#region Fields

	// Vertecies
	public List<Triangle> Triangles;
	public Vector3 Position;
	public Vector3 Rotation;

	#endregion
}