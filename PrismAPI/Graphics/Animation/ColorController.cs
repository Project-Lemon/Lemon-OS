namespace PrismAPI.Graphics.Animation;

/// <summary>
/// The <see cref="ColorController"/> class, used to add basic ease transitions to colors.
/// See: <seealso cref="AnimationController"/>
/// </summary>
public class ColorController : AnimationController
{
	#region Constructors

	/// <summary>
	/// Creates a new instance of the <see cref="ColorController"/> class.
	/// </summary>
	/// <param name="Source">The starting value.</param>
	/// <param name="Target">The value to end at.</param>
	/// <param name="Duration">The duration to play the animation over.</param>
	/// <param name="Mode">The ease animation mode.</param>
	public ColorController(uint Source, uint Target, TimeSpan Duration, AnimationMode Mode) : base(0f, 1f, Duration, Mode)
	{
		// Set-up all controllers for each color channel.
		this.Source = Source;
		this.Target = Target;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The current color defined by the animation.
	/// </summary>
	public new uint Current => Color32.Lerp(Source, Target, base.Current);

	#endregion

	#region Fields

	/// <summary>
	/// The source color of the animation.
	/// </summary>
	public new uint Source;

	/// <summary>
	/// The target color of the animation.
	/// </summary>
	public new uint Target;

	#endregion
}