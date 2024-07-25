using System.Drawing;
using System.Globalization;

namespace PrismAPI.Graphics;

/// <summary>
/// Color class, used for drawing.
/// </summary>
public static class Color32
{
	#region Methods

	/// <summary>
	/// Blends two colors together based on their alpha values.
	/// </summary>
	/// <param name="Source">The original color.</param>
	/// <param name="NewColor">The new color to mix.</param>
	/// <returns>Mixed color.</returns>
	public static uint AlphaBlend(uint Source, uint NewColor)
	{
		byte[] SplitSource = SplitChannels(Source);
		byte[] SplitNew = SplitChannels(NewColor);

		if (SplitNew[0] == 255)
		{
			return NewColor;
		}
		if (SplitNew[0] == 0)
		{
			return Source;
		}

		// Use explicit math here to decrease performance overhead and calculate it properly.
		return Color32.FromARGB(255,
			(int)SplitNew[1] + (int)(SplitSource[1] * SplitNew[0]) >> 8,
			(int)SplitNew[2] + (int)(SplitSource[2] * SplitNew[0]) >> 8,
			(int)SplitNew[3] + (int)(SplitSource[3] * SplitNew[0]) >> 8);
	}

	public static uint FromName(string ColorInfo)
	{
		// Check if input is invalid.
		if (string.IsNullOrEmpty(ColorInfo))
		{
			return 0;
		}

		// Create blank placeholders.
		int _A, _R, _G, _B;

		// Get CYMK color.
		if (ColorInfo.StartsWith("cymk("))
		{
			// Get individual components.
			string[] Components = ColorInfo[5..].Split(',');

			// Parse component data.
			byte C = byte.Parse(Components[0]);
			byte Y = byte.Parse(Components[1]);
			byte M = byte.Parse(Components[2]);
			byte K = byte.Parse(Components[3]);

			// Alpha is always 255 with CYMK.
			_A = 255;

			if (K != 255)
			{
				_R = (255 - C) * (255 - K) / 255;
				_G = (255 - M) * (255 - K) / 255;
				_B = (255 - Y) * (255 - K) / 255;
			}
			else
			{
				_R = 255 - C;
				_G = 255 - M;
				_B = 255 - Y;
			}

			// Assign the ARGB value.
			return Color32.FromARGB(_A, _R, _G, _B);
		}

		// Get ARGB color.
		if (ColorInfo.StartsWith("argb("))
		{
			// Check if value is packed.
			if (!ColorInfo.Contains(','))
			{
				return uint.Parse(ColorInfo[5..]);
			}

			// Get individual components.
			string[] Components = ColorInfo[5..].Split(',');

			// Parse component data.
			try
			{
				_A = byte.Parse(Components[0]);
				_R = byte.Parse(Components[1]);
				_G = byte.Parse(Components[2]);
				_B = byte.Parse(Components[3]);
			}
			catch
			{
				_A = (int)float.Parse(Components[0]);
				_R = (int)float.Parse(Components[1]);
				_G = (int)float.Parse(Components[2]);
				_B = (int)float.Parse(Components[3]);
			}

			// Assign the ARGB value.
			return Color32.FromARGB(_A, _R, _G, _B);
		}

		// Get RGB color.
		if (ColorInfo.StartsWith("rgb("))
		{
			// Get individual components.
			string[] Components = ColorInfo[5..].Split(',');

			// Alpha is always 255 with RGB.
			_A = 255;

			// Parse component data.
			try
			{
				_R = byte.Parse(Components[0]);
				_G = byte.Parse(Components[1]);
				_B = byte.Parse(Components[2]);
			}
			catch
			{
				_R = (int)float.Parse(Components[0]);
				_G = (int)float.Parse(Components[1]);
				_B = (int)float.Parse(Components[2]);
			}

			// Assign the ARGB value.
			return Color32.FromARGB(255, _R, _G, _B);
		}

		// Get HSV color.
		if (ColorInfo.StartsWith("hsl("))
		{
			// Get individual components.
			string[] Components = ColorInfo[5..].Split(',');

			// Alpha is always 100% with HSL.
			_A = 255;

			int H = (int)float.Parse(Components[0]);
			int S = (int)float.Parse(Components[1]);
			int L = (int)float.Parse(Components[2]);

			S = (int)Math.Clamp(S, 0.0, 1.0);
			L = (int)Math.Clamp(L, 0.0, 1.0);

			// Zero-saturation optimization.
			if (S == 0)
			{
				_R = L;
				_G = L;
				_B = L;

				return Color32.FromARGB(_A, _R, _G, _B);
			}

			float Q = L < 0.5 ? (L * S) + L : L + S - (L * S);
			float P = (2 * L) - Q;

			_R = (int)FromHue(P, Q, H + (1 / 3));
			_G = (int)FromHue(P, Q, H);
			_B = (int)FromHue(P, Q, H - (1 / 3));

			// Assign the ARGB value.
			return Color32.FromARGB(_A, _R, _G, _B);
		}

		// Get hex color.
		if (ColorInfo.StartsWith('#'))
		{
			// Get color with correct hex length.
			switch (ColorInfo.Length)
			{
				case 9:
					_A = byte.Parse(ColorInfo[1..3], NumberStyles.HexNumber);
					_R = byte.Parse(ColorInfo[3..5], NumberStyles.HexNumber);
					_G = byte.Parse(ColorInfo[5..7], NumberStyles.HexNumber);
					_B = byte.Parse(ColorInfo[7..9], NumberStyles.HexNumber);
					break;
				case 7:
					_A = 255;
					_R = byte.Parse(ColorInfo[1..3], NumberStyles.HexNumber);
					_G = byte.Parse(ColorInfo[3..5], NumberStyles.HexNumber);
					_B = byte.Parse(ColorInfo[5..7], NumberStyles.HexNumber);
					break;
				default:
					throw new FormatException("Hex value is not in correct format!");
			}

			// Assign the ARGB value.
			return Color32.FromARGB(_A, _R, _G, _B);
		}

		// Assume input is a color name.
		return ColorInfo switch
		{
			"AliceBlue" => 0xFFF0F8FF,
			"AntiqueWhite" => 0xFFFAEBD7,
			"Aqua" => 0xFF00FFFF,
			"Aquamarine" => 0xFF7FFFD4,
			"Azure" => 0xFFF0FFFF,
			"Beige" => 0xFFF5F5DC,
			"Bisque" => 0xFFFFE4C4,
			"Black" => 0xFF000000,
			"BlanchedAlmond" => 0xFFFFEBCD,
			"Blue" => 0xFF0000FF,
			"BlueViolet" => 0xFF8A2BE2,
			"Brown" => 0xFFA52A2A,
			"BurlyWood" => 0xFFDEB887,
			"CadetBlue" => 0xFF5F9EA0,
			"Chartreuse" => 0xFF7FFF00,
			"Chocolate" => 0xFFD2691E,
			"Coral" => 0xFFFF7F50,
			"CornflowerBlue" => 0xFF6495ED,
			"Cornsilk" => 0xFFFFF8DC,
			"Crimson" => 0xFFDC143C,
			"Cyan" => 0xFF00FFFF,
			"DarkBlue" => 0xFF00008B,
			"DarkCyan" => 0xFF008B8B,
			"DarkGoldenRod" => 0xFFB8860B,
			"DarkGray" => 0xFFA9A9A9,
			"DarkGrey" => 0xFFA9A9A9,
			"DarkGreen" => 0xFF006400,
			"DarkKhaki" => 0xFFBDB76B,
			"DarkMagenta" => 0xFF8B008B,
			"DarkOliveGreen" => 0xFF556B2F,
			"DarkOrange" => 0xFFFF8C00,
			"DarkOrchid" => 0xFF9932CC,
			"DarkRed" => 0xFF8B0000,
			"DarkSalmon" => 0xFFE9967A,
			"DarkSeaGreen" => 0xFF8FBC8F,
			"DarkSlateBlue" => 0xFF483D8B,
			"DarkSlateGray" => 0xFF2F4F4F,
			"DarkSlateGrey" => 0xFF2F4F4F,
			"DarkTurquoise" => 0xFF00CED1,
			"DarkViolet" => 0xFF9400D3,
			"DeepPink" => 0xFFFF1493,
			"DeepSkyBlue" => 0xFF00BFFF,
			"DimGray" => 0xFF696969,
			"DimGrey" => 0xFF696969,
			"DodgerBlue" => 0xFF1E90FF,
			"FireBrick" => 0xFFB22222,
			"FloralWhite" => 0xFFFFFAF0,
			"ForestGreen" => 0xFF228B22,
			"Fuchsia" => 0xFFFF00FF,
			"Gainsboro" => 0xFFDCDCDC,
			"GhostWhite" => 0xFFF8F8FF,
			"Gold" => 0xFFFFD700,
			"GoldenRod" => 0xFFDAA520,
			"Gray" => 0xFF808080,
			"Grey" => 0xFF808080,
			"Green" => 0xFF008000,
			"GreenYellow" => 0xFFADFF2F,
			"HoneyDew" => 0xFFF0FFF0,
			"HotPink" => 0xFFFF69B4,
			"IndianRed" => 0xFFCD5C5C,
			"Indigo" => 0xFF4B0082,
			"Ivory" => 0xFFFFFFF0,
			"Khaki" => 0xFFF0E68C,
			"Lavender" => 0xFFE6E6FA,
			"LavenderBlush" => 0xFFFFF0F5,
			"LawnGreen" => 0xFF7CFC00,
			"LemonChiffon" => 0xFFFFFACD,
			"LightBlue" => 0xFFADD8E6,
			"LightCoral" => 0xFFF08080,
			"LightCyan" => 0xFFE0FFFF,
			"LightGoldenRodYellow" => 0xFFFAFAD2,
			"LightGray" => 0xFFD3D3D3,
			"LightGrey" => 0xFFD3D3D3,
			"LightGreen" => 0xFF90EE90,
			"LightPink" => 0xFFFFB6C1,
			"LightSalmon" => 0xFFFFA07A,
			"LightSeaGreen" => 0xFF20B2AA,
			"LightSkyBlue" => 0xFF87CEFA,
			"LightSlateGray" => 0xFF778899,
			"LightSlateGrey" => 0xFF778899,
			"LightSteelBlue" => 0xFFB0C4DE,
			"LightYellow" => 0xFFFFFFE0,
			"Lime" => 0xFF00FF00,
			"LimeGreen" => 0xFF32CD32,
			"Linen" => 0xFFFAF0E6,
			"Magenta" => 0xFFFF00FF,
			"Maroon" => 0xFF800000,
			"MediumAquaMarine" => 0xFF66CDAA,
			"MediumBlue" => 0xFF0000CD,
			"MediumOrchid" => 0xFFBA55D3,
			"MediumPurple" => 0xFF9370DB,
			"MediumSeaGreen" => 0xFF3CB371,
			"MediumSlateBlue" => 0xFF7B68EE,
			"MediumSpringGreen" => 0xFF00FA9A,
			"MediumTurquoise" => 0xFF48D1CC,
			"MediumVioletRed" => 0xFFC71585,
			"MidnightBlue" => 0xFF191970,
			"MintCream" => 0xFFF5FFFA,
			"MistyRose" => 0xFFFFE4E1,
			"Moccasin" => 0xFFFFE4B5,
			"NavajoWhite" => 0xFFFFDEAD,
			"Navy" => 0xFF000080,
			"OldLace" => 0xFFFDF5E6,
			"Olive" => 0xFF808000,
			"OliveDrab" => 0xFF6B8E23,
			"Orange" => 0xFFFFA500,
			"OrangeRed" => 0xFFFF4500,
			"Orchid" => 0xFFDA70D6,
			"PaleGoldenRod" => 0xFFEEE8AA,
			"PaleGreen" => 0xFF98FB98,
			"PaleTurquoise" => 0xFFAFEEEE,
			"PaleVioletRed" => 0xFFDB7093,
			"PapayaWhip" => 0xFFFFEFD5,
			"PeachPuff" => 0xFFFFDAB9,
			"Peru" => 0xFFCD853F,
			"Pink" => 0xFFFFC0CB,
			"Plum" => 0xFFDDA0DD,
			"PowderBlue" => 0xFFB0E0E6,
			"Purple" => 0xFF800080,
			"RebeccaPurple" => 0xFF663399,
			"Red" => 0xFFFF0000,
			"RosyBrown" => 0xFFBC8F8F,
			"RoyalBlue" => 0xFF4169E1,
			"SaddleBrown" => 0xFF8B4513,
			"Salmon" => 0xFFFA8072,
			"SandyBrown" => 0xFFF4A460,
			"SeaGreen" => 0xFF2E8B57,
			"SeaShell" => 0xFFFFF5EE,
			"Sienna" => 0xFFA0522D,
			"Silver" => 0xFFC0C0C0,
			"SkyBlue" => 0xFF87CEEB,
			"SlateBlue" => 0xFF6A5ACD,
			"SlateGray" => 0xFF708090,
			"SlateGrey" => 0xFF708090,
			"Snow" => 0xFFFFFAFA,
			"SpringGreen" => 0xFF00FF7F,
			"SteelBlue" => 0xFF4682B4,
			"Tan" => 0xFFD2B48C,
			"Teal" => 0xFF008080,
			"Thistle" => 0xFFD8BFD8,
			"Tomato" => 0xFFFF6347,
			"Turquoise" => 0xFF40E0D0,
			"Violet" => 0xFFEE82EE,
			"Wheat" => 0xFFF5DEB3,
			"White" => 0xFFFFFFFF,
			"WhiteSmoke" => 0xFFF5F5F5,
			"Yellow" => 0xFFFFFF00,
			"YellowGreen" => 0xFF9ACD32,
			_ => throw new($"Color '{ColorInfo}' does not exist!"),
		};
	}

	/// <summary>
	/// Converts an ARGB color to it's packed ARGB format.
	/// </summary>
	/// <param name="A">Alpha channel.</param>
	/// <param name="R">Red channel.</param>
	/// <param name="G">Green channel.</param>
	/// <param name="B">Blue channel.</param>
	/// <returns>Packed value.</returns>
	public static uint FromARGB(float A, float R, float G, float B)
	{
		return BitConverter.ToUInt32(new byte[] { (byte)B, (byte)G, (byte)R, (byte)A });
	}

	/// <summary>
	/// This function returns one of the individual color channels of a color.
	/// <param name="Color">The color to extract</param>
	/// </summary>
	public static byte GetAlpha(uint Color)
	{
		return (byte)((Color >> 24) & 255);
	}

	/// <summary>
	/// This function returns one of the individual color channels of a color.
	/// <param name="Color">The color to extract</param>
	/// </summary>
	public static byte GetRed(uint Color)
	{
		return (byte)((Color >> 16) & 255);
	}

	/// <summary>
	/// This function returns one of the individual color channels of a color.
	/// <param name="Color">The color to extract</param>
	/// </summary>
	public static byte GetGreen(uint Color)
	{
		return (byte)((Color >> 8) & 255);
	}

	/// <summary>
	/// This function returns one of the individual color channels of a color.
	/// <param name="Color">The color to extract</param>
	/// </summary>
	public static byte GetBlue(uint Color)
	{
		return (byte)(Color & 255);
	}

	/// <summary>
	/// This function splits a color value into it's indiviual color components (A, R, G, B)
	///<param name="Color">The color to split.</param>
	/// </summary>
	public static byte[] SplitChannels(uint Color)
	{
		return new byte[] {
			GetAlpha(Color),
			GetRed(Color),
			GetGreen(Color),
			GetBlue(Color) };
	}

	/// <summary>
	/// Normalizes the color to be between 0.0 and 1.0.
	/// </summary>
	/// <returns>A normalized color.</returns>
	public static uint Normalize(uint ToNormalize)
	{
		// Don't use operators as to preserve the alpha value.
		return Divide(ToNormalize, 255);
	}

	/// <summary>
	/// Internal method, used by <see cref="FromHSL(float, float, float)"/>./>
	/// See: <seealso cref="https://github.com/CharlesStover/hsl2rgb-js/blob/master/src/hsl2rgb.js"/>
	/// </summary>
	/// <param name="P">Unknown.</param>
	/// <param name="Q">Unknown.</param>
	/// <param name="T">Unknown.</param>
	/// <returns>Unknown.</returns>
	private static float FromHue(float P, float Q, float T)
	{
		if (T < 0)
		{
			T++;
		}
		if (T > 1)
		{
			T--;
		}
		if (T < 1 / 6)
		{
			return P + ((Q - P) * 6 * T);
		}
		if (T < 0.5)
		{
			return Q;
		}
		if (T < 2 / 3)
		{
			return P + ((Q - P) * ((2 / 3) - T) * 6);
		}

		return P;
	}

	/// <summary>
	/// Inverts the specified color.
	/// </summary>
	/// <param name="ToInvert">The color that will be inverted.</param>
	/// <returns>An inverted variant of the input.</returns>
	public static uint Invert(uint ToInvert)
	{
		return Subtract(White, ToInvert);
	}

	/// <summary>
	/// The function to linearly interpolate between 2 colors. (32-bit)
	/// </summary>
	/// <param name="StartValue">The color to start with.</param>
	/// <param name="EndValue">The color to end with.</param>
	/// <param name="Index">Any number between 0.0 and 1.0.</param>
	/// <returns>The value between 'StartValue' and 'EndValue' as marked by 'Index'.</returns>
	public static uint Lerp(uint StartValue, uint EndValue, float Index)
	{
		// Ensure 'Index' is between 0.0 and 1.0.
		Index = (float)Math.Clamp(Index, 0.0, 1.0);

		byte[] SplitStart = SplitChannels(StartValue);
		byte[] SplitEnd = SplitChannels(EndValue);

		// Can we simplify this?
		return FromARGB(
			Add(SplitStart[0], (Multiply((uint)(SplitEnd[0] - SplitStart[0]), Index))),
			Add(SplitStart[1], (Multiply((uint)(SplitEnd[1] - SplitStart[1]), Index))),
			Add(SplitStart[2], (Multiply((uint)(SplitEnd[2] - SplitStart[2]), Index))),
			Add(SplitStart[3], (Multiply((uint)(SplitEnd[3] - SplitStart[3]), Index)))
		);
	}

	public static uint Add(uint Color1, uint Color2)
	{
		byte[] C1S = SplitChannels(Color1);
		byte[] C2S = SplitChannels(Color2);

		return FromARGB(
			C1S[0] + C2S[0],
			C1S[1] + C2S[1],
			C1S[2] + C2S[2],
			C1S[3] + C2S[3]
		);
	}

	public static uint Add(uint Color1, float Value)
	{
		byte[] C1S = SplitChannels(Color1);

		return FromARGB(
			C1S[0] + Value,
			C1S[1] + Value,
			C1S[2] + Value,
			C1S[3] + Value
		);
	}

	public static uint Subtract(uint Color1, uint Color2)
	{
		byte[] C1S = SplitChannels(Color1);
		byte[] C2S = SplitChannels(Color2);

		return FromARGB(
			C1S[0] - C2S[0],
			C1S[1] - C2S[1],
			C1S[2] - C2S[2],
			C1S[3] - C2S[3]
		);
	}

	public static uint Multiply(uint Color1, uint Color2)
	{
		byte[] C1S = SplitChannels(Color1);
		byte[] C2S = SplitChannels(Color2);

		return FromARGB(
			C1S[0] * C2S[0],
			C1S[1] * C2S[1],
			C1S[2] * C2S[2],
			C1S[3] * C2S[3]
		);
	}

	public static uint Multiply(uint Color1, float Value)
	{
		byte[] C1S = SplitChannels(Color1);

		return FromARGB(
			C1S[0] * Value,
			C1S[1] * Value,
			C1S[2] * Value,
			C1S[3] * Value
		);
	}

	public static uint Divide(uint Color1, uint Color2)
	{
		byte[] C1S = SplitChannels(Color1);
		byte[] C2S = SplitChannels(Color2);

		return FromARGB(
			C1S[0] / C2S[0],
			C1S[1] / C2S[1],
			C1S[2] / C2S[2],
			C1S[3] / C2S[3]
		);
	}

	public static uint Divide(uint Color1, float Value)
	{
		byte[] C1S = SplitChannels(Color1);

		return FromARGB(
			C1S[0] / Value,
			C1S[1] / Value,
			C1S[2] / Value,
			C1S[3] / Value
		);
	}

	#endregion

	#region Fields

	public static readonly uint White = FromARGB(255, 255, 255, 255);
	public static readonly uint Black = FromARGB(255, 0, 0, 0);
	public static readonly uint Cyan = FromARGB(255, 0, 255, 255);
	public static readonly uint Red = FromARGB(255, 255, 0, 0);
	public static readonly uint Green = FromARGB(255, 0, 255, 0);
	public static readonly uint Blue = FromARGB(255, 0, 0, 255);
	public static readonly uint CoolGreen = FromARGB(255, 54, 94, 53);
	public static readonly uint Magenta = FromARGB(255, 255, 0, 255);
	public static readonly uint Yellow = FromARGB(255, 255, 255, 0);
	public static readonly uint HotPink = FromARGB(255, 230, 62, 109);
	public static readonly uint UbuntuPurple = FromARGB(255, 66, 5, 22);
	public static readonly uint GoogleBlue = FromARGB(255, 66, 133, 244);
	public static readonly uint GoogleGreen = FromARGB(255, 52, 168, 83);
	public static readonly uint GoogleYellow = FromARGB(255, 251, 188, 5);
	public static readonly uint GoogleRed = FromARGB(255, 234, 67, 53);
	public static readonly uint DeepOrange = FromARGB(255, 255, 64, 0);
	public static readonly uint RubyRed = FromARGB(255, 204, 52, 45);
	public static readonly uint Transparent = FromARGB(0, 0, 0, 0);
	public static readonly uint StackOverflowOrange = FromARGB(255, 244, 128, 36);
	public static readonly uint StackOverflowBlack = FromARGB(255, 34, 36, 38);
	public static readonly uint StackOverflowWhite = FromARGB(255, 188, 187, 187);
	public static readonly uint DeepGray = FromARGB(255, 25, 25, 25);
	public static readonly uint LightGray = FromARGB(255, 125, 125, 125);
	public static readonly uint SuperOrange = FromARGB(255, 255, 99, 71);
	public static readonly uint FakeGrassGreen = FromARGB(255, 60, 179, 113);
	public static readonly uint DeepBlue = FromARGB(255, 51, 47, 208);
	public static readonly uint BloodOrange = FromARGB(255, 255, 123, 0);
	public static readonly uint LightBlack = FromARGB(255, 25, 25, 25);
	public static readonly uint LighterBlack = FromARGB(255, 50, 50, 50);
	public static readonly uint ClassicBlue = FromARGB(255, 52, 86, 139);
	public static readonly uint LivingCoral = FromARGB(255, 255, 111, 97);
	public static readonly uint UltraViolet = FromARGB(255, 107, 91, 149);
	public static readonly uint Greenery = FromARGB(255, 136, 176, 75);
	public static readonly uint Emerald = FromARGB(255, 0, 155, 119);
	public static readonly uint LightPurple = 0xFFA0A5DDu;
	public static readonly uint Minty = 0xFF74C68Bu;
	public static readonly uint SunsetRed = 0xFFE07572u;
	public static readonly uint LightYellow = 0xFFF9C980u;

	#endregion
}