using System.Numerics;

namespace PrismAPI.Graphics.Rasterizer;

public class Triangle
{
	#region Constructors

	public Triangle(float X1, float Y1, float Z1, float X2, float Y2, float Z2, float X3, float Y3, float Z3, uint Color)
	{
		// Assign current points.
		P1 = new(X1, Y1, Z1);
		P2 = new(X2, Y2, Z2);
		P3 = new(X3, Y3, Z3);

		// Assign the color value.
		this.Color = Color;

		// Assign other data.
		T1 = Vector3.Zero;
		T2 = Vector3.Zero;
		T3 = Vector3.Zero;
		L1 = Vector3.Zero;
		L2 = Vector3.Zero;
		L3 = Vector3.Zero;
	}
	public Triangle(Vector3 P1, Vector3 P2, Vector3 P3, uint Color)
	{
		// Assign current points.
		this.P1 = P1;
		this.P2 = P2;
		this.P3 = P3;

		// Assign the color value.
		this.Color = Color;

		// Assign other data.
		T1 = Vector3.Zero;
		T2 = Vector3.Zero;
		T3 = Vector3.Zero;
		L1 = Vector3.Zero;
		L2 = Vector3.Zero;
		L3 = Vector3.Zero;
	}
	public Triangle()
	{
		Color = Color32.Black;
		P1 = Vector3.Zero;
		P2 = Vector3.Zero;
		P3 = Vector3.Zero;
		T1 = Vector3.Zero;
		T2 = Vector3.Zero;
		T3 = Vector3.Zero;
		L1 = Vector3.Zero;
		L2 = Vector3.Zero;
		L3 = Vector3.Zero;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The Height in normal screen space of the triangle.
	/// </summary>
	public float Height
	{
		get
		{
			// Get the closest top and bottom components.
			float Top = MathF.Min(MathF.Min(P1.Y, P2.Y), P3.Y);
			float Bottom = MathF.Max(MathF.Max(P1.Y, P2.Y), P3.Y);

			return Bottom - Top;
		}
	}

	/// <summary>
	/// The Width in normal screen space of the triangle.
	/// </summary>
	public float Width
	{
		get
		{
			// Get the closest left and right components.
			float Left = MathF.Min(MathF.Min(P1.X, P2.X), P3.X);
			float Right = MathF.Max(MathF.Max(P1.X, P2.X), P3.X);

			return Right - Left;
		}
	}

	#endregion

	#region Methods

	/// <summary>
	/// Transforms the triangle with the standard vector transformation formula.
	/// </summary>
	/// <param name="Triangle">The <see cref="Triangle"/> to transform.</param>
	/// <param name="Transformation">The <see cref="Quaternion"/> to transform with.</param>
	/// <returns>The transformed triangle.</returns>
	public static Triangle Transform(Triangle Triangle, Quaternion Transformation)
	{
		return new()
		{
			// Assign new point values.
			P1 = Vector3.Transform(Triangle.P1, Transformation),
			P2 = Vector3.Transform(Triangle.P2, Transformation),
			P3 = Vector3.Transform(Triangle.P3, Transformation),

			// Copy existing values.
			Color = Triangle.Color,
			T1 = Triangle.T1,
			T2 = Triangle.T2,
			T3 = Triangle.T3,
			L1 = Triangle.L1,
			L2 = Triangle.L2,
			L3 = Triangle.L3,
		};
	}

	/// <summary>
	/// Transforms the triangle with the standard vector transformation formula.
	/// </summary>
	/// <param name="Triangle">The <see cref="Triangle"/> to transform.</param>
	/// <param name="Transformation">The <see cref="Matrix4x4"/> to transform with.</param>
	/// <returns>The transformed triangle.</returns>
	public static Triangle Transform(Triangle Triangle, Matrix4x4 Transformation)
	{
		return new()
		{
			// Assign new point values.
			P1 = Vector3.Transform(Triangle.P1, Transformation),
			P2 = Vector3.Transform(Triangle.P2, Transformation),
			P3 = Vector3.Transform(Triangle.P3, Transformation),

			// Copy existing values.
			Color = Triangle.Color,
			T1 = Triangle.T1,
			T2 = Triangle.T2,
			T3 = Triangle.T3,
			L1 = Triangle.L1,
			L2 = Triangle.L2,
			L3 = Triangle.L3,
		};
	}

	/// <summary>
	/// Translates or "moves" the triangle based on the input translation.
	/// </summary>
	/// <param name="Triangle">The <see cref="Triangle"/> to transform.</param>
	/// <param name="Translation">The translation to move the triangle by.</param>
	/// <returns>Translated triangle.</returns>
	public static Triangle Translate(Triangle Triangle, Vector3 Translation)
	{
		return new()
		{
			// Assign new point values.
			P1 = Vector3.Add(Triangle.P1, Translation),
			P2 = Vector3.Add(Triangle.P2, Translation),
			P3 = Vector3.Add(Triangle.P3, Translation),

			// Copy existing values.
			Color = Triangle.Color,
			T1 = Triangle.T1,
			T2 = Triangle.T2,
			T3 = Triangle.T3,
			L1 = Triangle.L1,
			L2 = Triangle.L2,
			L3 = Triangle.L3,
		};
	}

	/// <summary>
	/// Multiplies the triangle by a translator 'matrix' - It is simpler than using a normal matrix.
	/// </summary>
	/// <param name="Triangle">The <see cref="Triangle"/> to transform.</param>
	/// <param name="Translator">The translator to use.</param>
	/// <returns>A translated triangle, as defined by the input.</returns>
	public static Triangle Translate(Triangle Triangle, float Translator)
	{
		float Cache1 = Translator / (Translator + Triangle.P1.Z);
		float Cache2 = Translator / (Translator + Triangle.P2.Z);
		float Cache3 = Translator / (Translator + Triangle.P3.Z);

		Vector3 M1 = new(Cache1, Cache1, 1);
		Vector3 M2 = new(Cache2, Cache2, 1);
		Vector3 M3 = new(Cache3, Cache3, 1);

		return new()
		{
			// Assign new point values.
			P1 = Vector3.Multiply(Triangle.P1, M1),
			P2 = Vector3.Multiply(Triangle.P2, M2),
			P3 = Vector3.Multiply(Triangle.P3, M3),

			// Copy existing values.
			Color = Triangle.Color,
			T1 = Triangle.T1,
			T2 = Triangle.T2,
			T3 = Triangle.T3,
			L1 = Triangle.L1,
			L2 = Triangle.L2,
			L3 = Triangle.L3,
		};
	}

	/// <summary>
	/// Center the trangle in the screen.
	/// </summary>
	/// <param name="Triangle">The <see cref="Triangle"/> to transform.</param>
	/// <param name="Width">The render screen pixel width.</param>
	/// <param name="Height">The render screen pixel height.</param>
	/// <returns>Centered triangle.</returns>
	public static Triangle Center(Triangle Triangle, uint Width, uint Height)
	{
		return new()
		{
			// Assign new point values.
			P1 = Vector3.Add(Triangle.P1, new(Width / 2, Height / 2, 0)),
			P2 = Vector3.Add(Triangle.P2, new(Width / 2, Height / 2, 0)),
			P3 = Vector3.Add(Triangle.P3, new(Width / 2, Height / 2, 0)),

			// Copy existing values.
			Color = Triangle.Color,
			T1 = Triangle.T1,
			T2 = Triangle.T2,
			T3 = Triangle.T3,
			L1 = Triangle.L1,
			L2 = Triangle.L2,
			L3 = Triangle.L3,
		};
	}

	/// <summary>
	/// Gets the normal value of the triangle.
	/// </summary>
	/// <returns>Normal of the triangle.</returns>
	public float GetNormal()
	{
		return
            ((P2.X - P1.X) *
			(P3.Y - P1.Y)) -
            ((P2.Y - P1.Y) *
			(P3.X - P1.X));
	}

	#endregion

	#region Fields

	/// <summary>
	/// A point of the triangle.
	/// </summary>
	public Vector3 P1, P2, P3;

	/// <summary>
	/// A texture point of the triangle.
	/// </summary>
	public Vector3 T1, T2, T3;

	/// <summary>
	/// A light point of the triangle.
	/// </summary>
	public Vector3 L1, L2, L3;

	/// <summary>
	/// The Color of the triangle.
	/// </summary>
	public uint Color;

	#endregion
}