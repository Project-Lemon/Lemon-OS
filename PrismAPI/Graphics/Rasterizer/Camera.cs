using System.Numerics;

namespace PrismAPI.Graphics.Rasterizer;

/// <summary>
/// The camera class, only used to represent location and rotation of a camera.
/// </summary>
public class Camera
{
	#region Constructors

	/// <summary>
	/// Creates a new instance of the <see cref="Camera"/> class.
	/// </summary>
	/// <param name="FOV">The FOV of the camera.</param>
	public Camera(float FOV = 75f)
	{
		MovementSpeed = 10f;
		Sensitivity = .75f;
		Position = new();
		Rotation = new();
		Ambient = Color32.White;
		this.FOV = FOV;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets the rotation Quaternion for the camera projection.
	/// </summary>
	/// <returns>The rotation Quaternion as specified by the camera's rotation.</returns>
	public Quaternion GetRotationQuaternion()
	{
		return Quaternion.CreateFromYawPitchRoll(Rotation.X, Math.Clamp(Rotation.Y, -90f, 90f), Rotation.Z);
	}

	/// <summary>
	/// Gets the mouse to camera movement vector based on mouse inputs.
	/// </summary>
	/// <param name="DeltaX">The delta 'X' value of the mouse.</param>
	/// <param name="DeltaY">The delta 'Y' value of the mouse.</param>
	/// <param name="Width">The width of the canvas.</param>
	/// <param name="Height">The height of the canvas.</param>
	/// <returns>The rotation axis that the camera should add to.</returns>
	public Vector3 GetRotationAxis(float DeltaX, float DeltaY, ushort Width, ushort Height)
	{
		return new()
		{
			X = DeltaX != 0 ? -DeltaX * Sensitivity / Width : 0,
			Y = DeltaY != 0 ? DeltaY * Sensitivity / Height : 0,
			Z = 0,
		};
	}

	#endregion

	#region Fields

	public float MovementSpeed;
	public float Sensitivity;
	public Vector3 Position;
	public Vector3 Rotation;
	public uint Ambient;
	public float FOV;

	#endregion
}