using System.Numerics;

namespace PrismAPI.Graphics.Rasterizer;

/// <summary>
/// The <see cref="Light"/> class, used to represent a light's type, position, rotation, and color.
/// </summary>
public class Light
{
	#region Constructors

	/// <summary>
	/// Creates a new instance of the <see cref="Light"/> class.
	/// </summary>
	/// <param name="Position">The position of the light source.</param>
	/// <param name="Rotation">The rotation of the light source.</param>
	/// <param name="Type">The kind of light to render.</param>
	/// <param name="Color">The color of the light source.</param>
	public Light(Vector3 Position, Vector3 Rotation, LightTypes Type, uint Color)
	{
		this.Position = Position;
		this.Rotation = Rotation;
		this.Color = Color;
		this.Type = Type;
	}

	#endregion

	#region Fields

	public Vector3 Position;
	public Vector3 Rotation;
	public LightTypes Type;
	public uint Color;

	#endregion
}