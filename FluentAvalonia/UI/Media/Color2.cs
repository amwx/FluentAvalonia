using Avalonia.Media;
using FluentAvalonia.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace FluentAvalonia.UI.Media
{
    internal enum ColorType
    {
        Undefined,
        RGB,
        HSV,
        HSL,
		CMYK
    }

    [TypeConverter(typeof(Color2ToColorConverter))]
    public struct Color2 : IEquatable<Color2>
    {
		/// <summary>
		/// Creates a RGB color 2 from the given RGBA values
		/// </summary>
		/// <param name="r">Red, [0,255]</param>
		/// <param name="g">Green, [0,255]</param>
		/// <param name="b">Blue, [0,255]</param>
		/// <param name="a">Alpha, [0,255]</param>
		public Color2(byte r, byte g, byte b, byte a = 255)
		{
			_cType = ColorType.RGB;
			_c1 = r / 255f;
			_c2 = g / 255f;
			_c3 = b / 255f;
			_c4 = 0;
			_alpha = a / 255f;
		}

		/// <summary>
		/// Creates a RGB Color2 from an <see cref="Avalonia.Media.Color"/>
		/// </summary>
		/// <param name="avColor"></param>
		public Color2(Color avColor)
		{
			_cType = ColorType.RGB;
			_c1 = avColor.R / 255;
			_c2 = avColor.G / 255;
			_c3 = avColor.B / 255;
			_c4 = 0;
			_alpha = avColor.A / 255;
		}


		/// <summary>
		/// Gets the Alpha channel of the color, [0,255]
		/// </summary>
		public byte A
		{
			get
			{
				return (byte)Math.Round(_alpha * 255);
			}
		}

		/// <summary>
		/// Gets the Red channel of the color. If color is not an RGB color, it is converted to one. [0,255]
		/// </summary>
		public byte R
		{
			get
			{
				if (_cType != ColorType.RGB)
				{
					return ToRGB().R;
				}

				return (byte)Math.Round(_c1 * 255);
			}
		}

		/// <summary>
		/// Gets the Green channel of the color. If color is not an RGB color, it is converted to one. [0,255]
		/// </summary>
		public byte G
		{
			get
			{
				if (_cType != ColorType.RGB)
				{
					return ToRGB().G;
				}

				return (byte)Math.Round(_c2 * 255);
			}
		}

		/// <summary>
		/// Gets the Blue channel of the color. If color is not an RGB color, it is converted to one. [0,255]
		/// </summary>
		public byte B
		{
			get
			{
				if (_cType != ColorType.RGB)
				{
					return ToRGB().B;
				}

				return (byte)Math.Round(_c3 * 255);
			}
		}

		/// <summary>
		/// Gets the Alpha channel of the color. [0,1]
		/// </summary>
		public float Af => _alpha;

		/// <summary>
		/// Gets the Red channel of the color. If color is not an RGB color, it is converted to one. [0,1]
		/// </summary>
		public float Rf
		{
			get
			{
				if (_cType != ColorType.RGB)
				{
					return ToRGB().Rf;
				}

				return _c1;
			}
		}

		/// <summary>
		/// Gets the Green channel of the color. If color is not an RGB color, it is converted to one. [0,1]
		/// </summary>
		public float Gf
		{
			get
			{
				if (_cType != ColorType.RGB)
				{
					return ToRGB().Gf;
				}

				return _c2;
			}
		}

		/// <summary>
		/// Gets the Blue channel of the color. If color is not an RGB color, it is converted to one. [0,1]
		/// </summary>
		public float Bf
		{
			get
			{
				if (_cType != ColorType.RGB)
				{
					return ToRGB().Bf;
				}

				return _c3;
			}
		}

		/// <summary>
		/// Gets the HSL or HSV Hue of the color. HSL and HSV hue is the same, so if the color type is neither HSV
		/// or HSL, it is converted to HSV first. [0,360)
		/// </summary>
		public int Hue
		{
			get
			{
				//HSL & HSV Hue is the same, so it doesn't matter which we refer to
				if (_cType != ColorType.HSV && _cType != ColorType.HSL)
				{
					return ToHSV().Hue;
				}

				return (int)Math.Round(_c1);
			}
		}

		/// <summary>
		/// Gets the HSL or HSV Hue of the color. HSL and HSV hue is the same, so if the color type is neither HSV
		/// or HSL, it is converted to HSV first. [0,360)
		/// </summary>
		public float Huef
		{
			get
			{
				//HSL & HSV Hue is the same, so it doesn't matter which we refer to
				if (_cType != ColorType.HSV && _cType != ColorType.HSL)
				{
					return ToHSV().Huef;
				}

				return _c1;
			}
		}

		/// <summary>
		/// Gets the HSV Saturation of the color. If the color is not an HSV color, it is converted first. [0,100]
		/// </summary>
		public int Saturation
		{
			get
			{
				if (_cType != ColorType.HSV)
				{
					return ToHSV().Saturation;
				}

				return (int)Math.Round(_c2 * 100);
			}
		}

		/// <summary>
		/// Gets the HSV Saturation of the color. If the color is not an HSV color, it is converted first. [0,1]
		/// </summary>
		public float Saturationf
		{
			get
			{
				if (_cType != ColorType.HSV)
				{
					return ToHSV().Saturationf;
				}

				return _c2;
			}
		}

		/// <summary>
		/// Gets the Value of the color. If the color is not an HSV color, it is converted first. [0,100]
		/// </summary>
		public int Value
		{
			get
			{
				if (_cType != ColorType.HSV)
				{
					return ToHSV().Value;
				}

				return (int)Math.Round(_c3 * 100);
			}
		}

		/// <summary>
		/// Gets the HSV Value of the color. If the color is not an HSV color, it is converted first. [0,1]
		/// </summary>
		public float Valuef
		{
			get
			{
				if (_cType != ColorType.HSV)
				{
					return ToHSV().Valuef;
				}

				return _c3;
			}
		}

		/// <summary>
		/// Gets the HSL Saturation of the color. If the color is not an HSL color, it is converted first. [0,100]
		/// </summary>
		public int HSLSaturation
		{
			get
			{
				if (_cType != ColorType.HSL)
				{
					return ToHSL().HSLSaturation;
				}

				return (int)Math.Round(_c2 * 100);
			}
		}

		/// <summary>
		/// Gets the HSL Saturation of the color. If the color is not an HSL color, it is converted first. [0,1]
		/// </summary>
		public float HSLSaturationf
		{
			get
			{
				if (_cType != ColorType.HSL)
				{
					return ToHSL().HSLSaturationf;
				}

				return _c2;
			}
		}

		/// <summary>
		/// Gets the HSL Lightness of the color. If the color is not an HSL color, it is converted first. [0,100]
		/// </summary>
		public int Lightness
		{
			get
			{
				if (_cType != ColorType.HSL)
				{
					return ToHSL().Lightness;
				}

				return (int)Math.Round(_c3 * 100);
			}
		}

		/// <summary>
		/// Gets the HSL Lightness of the color. If the color is not an HSL color, it is converted first. [0,1]
		/// </summary>
		public float Lightnessf
		{
			get
			{
				if (_cType != ColorType.HSL)
				{
					return ToHSL().Lightnessf;
				}

				return _c3;
			}
		}

		/// <summary>
		/// Gets the CMYK Cyan of the color. If the color is not an CMYK color, it is converted first. [0,100]
		/// </summary>
		public int CMYKCyan
		{
			get
			{
				if (_cType != ColorType.CMYK)
				{
					return ToCMYK().CMYKCyan;
				}

				return (int)Math.Round(_c1 * 100);
			}
		}

		/// <summary>
		/// Gets the CMYK Cyan of the color. If the color is not an CMYK color, it is converted first. [0,1]
		/// </summary>
		public float CMYKCyanf
		{
			get
			{
				if (_cType != ColorType.CMYK)
				{
					return ToCMYK().CMYKCyanf;
				}

				return _c1;
			}
		}

		/// <summary>
		/// Gets the CMYK Magenta of the color. If the color is not an CMYK color, it is converted first. [0,100]
		/// </summary>
		public int CMYKMagenta
		{
			get
			{
				if (_cType != ColorType.CMYK)
				{
					return ToCMYK().CMYKMagenta;
				}

				return (int)Math.Round(_c2 * 100);
			}
		}

		/// <summary>
		/// Gets the CMYK Magenta of the color. If the color is not an CMYK color, it is converted first. [0,1]
		/// </summary>
		public float CMYKMagentaf
		{
			get
			{
				if (_cType != ColorType.CMYK)
				{
					return ToCMYK().CMYKMagentaf;
				}

				return _c2;
			}
		}

		/// <summary>
		/// Gets the CMYK Yellow of the color. If the color is not an CMYK color, it is converted first. [0,100]
		/// </summary>
		public int CMYKYellow
		{
			get
			{
				if (_cType != ColorType.CMYK)
				{
					return ToCMYK().CMYKYellow;
				}

				return (int)Math.Round(_c3 * 100);
			}
		}

		/// <summary>
		/// Gets the CMYK Yellow of the color. If the color is not an CMYK color, it is converted first. [0,1]
		/// </summary>
		public float CMYKYellowf
		{
			get
			{
				if (_cType != ColorType.CMYK)
				{
					return ToCMYK().CMYKYellowf;
				}

				return _c3;
			}
		}

		/// <summary>
		/// Gets the CMYK Black of the color. If the color is not an CMYK color, it is converted first. [0,100]
		/// </summary>
		public int CMYKBlack
		{
			get
			{
				if (_cType != ColorType.CMYK)
				{
					return ToCMYK().CMYKBlack;
				}

				return (int)Math.Round(_c4 * 100);
			}
		}

		/// <summary>
		/// Gets the CMYK Black of the color. If the color is not an CMYK color, it is converted first. [0,1]
		/// </summary>
		public float CMYKBlackf
		{
			get
			{
				if (_cType != ColorType.CMYK)
				{
					return ToCMYK().CMYKBlackf;
				}

				return _c4;
			}
		}


		/// <summary>
		/// Gets all RGBA components of the color. If not RGB, color is converted first
		/// </summary>
		/// <param name="r">Red, [0,255]</param>
		/// <param name="g">Green, [0,255]</param>
		/// <param name="b">Blue, [0,255]</param>
		/// <param name="a">Alpha, [0,255]</param>
		public void GetRGB(out byte r, out byte g, out byte b, out byte a)
		{
			if (_cType != ColorType.RGB)
			{
				ToRGB().GetRGB(out r, out g, out b, out a);
				return;
			}

			r = (byte)Math.Round(_c1 * 255);
			g = (byte)Math.Round(_c2 * 255);
			b = (byte)Math.Round(_c3 * 255);
			a = (byte)Math.Round(_alpha * 255);
		}

		/// <summary>
		/// Gets all RGBA components of the color as floating point numbers. If not RGB, color is converted first
		/// </summary>
		/// <param name="r">Red, [0,1]</param>
		/// <param name="g">Green, [0,1]</param>
		/// <param name="b">Blue, [0,1]</param>
		/// <param name="a">Alpha, [0,1]</param>
		public void GetRGBf(out float r, out float g, out float b, out float a)
		{
			if (_cType != ColorType.RGB)
			{
				ToRGB().GetRGBf(out r, out g, out b, out a);
				return;
			}

			r = _c1;
			g = _c2;
			b = _c3;
			a = _alpha;
		}

		/// <summary>
		/// Gets all HSV components of the color as floating point numbers. If not HSV, color is converted first
		/// </summary>
		/// <param name="h">Hue, [0,360)</param>
		/// <param name="s">Saturation, [0,1]</param>
		/// <param name="v">Value, [0,1]</param>
		/// <param name="a">Alpha, [0,1]</param>
		public void GetHSVf(out float h, out float s, out float v, out float a)
		{
			if (_cType != ColorType.HSV)
			{
				ToHSV().GetHSVf(out h, out s, out v, out a);
				return;
			}

			h = _c1;
			s = _c2;
			v = _c3;
			a = _alpha;
		}

		/// <summary>
		/// Gets all HSV components of the color. If not HSV, color is converted first
		/// </summary>
		/// <param name="h">Hue, [0,360)</param>
		/// <param name="s">Saturation, [0,100]</param>
		/// <param name="v">Value, [0,100]</param>
		/// <param name="a">Alpha, [0,255]</param>
		public void GetHSV(out int h, out int s, out int v, out int a)
		{
			if (_cType != ColorType.HSV)
			{
				ToHSV().GetHSV(out h, out s, out v, out a);
				return;
			}

			h = (int)Math.Round(_c1);
			s = (int)Math.Round(_c2 * 100);
			v = (int)Math.Round(_c3 * 100);
			a = (int)Math.Round(_alpha * 255);
		}
		
		/// <summary>
		/// Gets all HSL components of the color as floating point numbers. If not HSL, color is converted first
		/// </summary>
		/// <param name="h">Hue, [0,360)</param>
		/// <param name="s">Saturation, [0,1]</param>
		/// <param name="l">Lightness, [0,1]</param>
		/// <param name="a">Alpha, [0,1]</param>
		public void GetHSLf(out float h, out float s, out float l, out float a)
		{
			if (_cType != ColorType.HSL)
			{
				ToHSL().GetHSLf(out h, out s, out l, out a);
				return;
			}

			h = _c1;
			s = _c2;
			l = _c3;
			a = _alpha;
		}

		/// <summary>
		/// Gets all HSL components of the color. If not HSL, color is converted first
		/// </summary>
		/// <param name="h">Hue, [0,360)</param>
		/// <param name="s">Saturation, [0,100]</param>
		/// <param name="v">Lightness, [0,100]</param>
		/// <param name="a">Alpha, [0,255]</param>
		public void GetHSL(out int h, out int s, out int l, out int a)
		{
			if (_cType != ColorType.HSL)
			{
				ToHSL().GetHSL(out h, out s, out l, out a);
				return;
			}

			h = (int)Math.Round(_c1);
			s = (int)Math.Round(_c2 * 100);
			l = (int)Math.Round(_c3 * 100);
			a = (int)Math.Round(_alpha * 255);
		}

		/// <summary>
		/// Gets all CMYK components of the color as floating point numbers. If not CMYK, color is converted first
		/// </summary>
		/// <param name="c">Cyan, [0,1]</param>
		/// <param name="m">Magenta, [0,1]</param>
		/// <param name="y">Yellow, [0,1]</param>
		/// <param name="k">Black, [0,1]</param>
		/// <param name="a">Alpha, [0,1]</param>
		public void GetCMYKf(out float c, out float m, out float y, out float k, out float a)
		{
			if (_cType != ColorType.CMYK)
			{
				ToCMYK().GetCMYKf(out c, out m, out y, out k, out a);
				return;
			}

			c = _c1;
			m = _c2;
			y = _c3;
			k = _c4;
			a = _alpha;
		}

		/// <summary>
		/// Gets all CMYK components of the color. If not CMYK, color is converted first
		/// </summary>
		/// <param name="c">Cyan, [0,100]</param>
		/// <param name="m">Magenta, [0,100]</param>
		/// <param name="y">Yellow, [0,100]</param>
		/// <param name="k">Black, [0,100]</param>
		/// <param name="a">Alpha, [0,255]</param>
		public void GetCMYK(out int c, out int m, out int y, out int k, out int a)
		{
			if (_cType != ColorType.CMYK)
			{
				ToCMYK().GetCMYK(out c, out m, out y, out k, out a);
				return;
			}

			c = (int)Math.Round(_c1 * 100);
			m = (int)Math.Round(_c2 * 100);
			y = (int)Math.Round(_c3 * 100);
			k = (int)Math.Round(_c4 * 100);
			a = (int)Math.Round(_alpha * 255);
		}

		/// <summary>
		/// Converts the current color to RGB color space
		/// </summary>
		/// <returns>RGB <see cref="Color2"/></returns>
		public Color2 ToRGB()
		{
			if (_cType == ColorType.RGB)
				return this;
			
			float r = 0;
			float g = 0;
			float b = 0;
			switch (_cType)
			{
				case ColorType.HSV:
					HSVToRGB(_c1, _c2, _c3, out r, out g, out b);
					break;

				case ColorType.HSL:
					HSLToRGB(_c1, _c2, _c3, out r, out g, out b);
					break;

				case ColorType.CMYK:
					CMYKToRGB(_c1, _c2, _c3, _c4, out r, out g, out b);
					break;
			}

			Color2 newColor = new Color2();
			newColor._cType = ColorType.RGB;
			newColor._c1 = r;
			newColor._c2 = g;
			newColor._c3 = b;
			newColor._alpha = _alpha;

			return newColor;
		}

		/// <summary>
		/// Converts the current color to HSV color space
		/// </summary>
		/// <returns>HSV <see cref="Color2"/></returns>
		public Color2 ToHSV()
		{
			if (_cType == ColorType.HSV)
				return this;

			float h = 0;
			float s = 0;
			float v = 0;
			switch (_cType)
			{
				case ColorType.RGB:
					RGBToHSV(_c1, _c2, _c3, out h, out s, out v);
					break;

				case ColorType.HSL:
					h = _c1;
					HSLToHSV(_c2, _c3, out s, out v);
					break;

				case ColorType.CMYK:
					//Only support RGB <-> CMYK, so convert & return;
					ToRGB().GetHSVf(out h, out s, out v, out _);
					break;
			}

			Color2 newColor = new Color2();
			newColor._cType = ColorType.HSV;
			newColor._c1 = h;
			newColor._c2 = s;
			newColor._c3 = v;
			newColor._alpha = _alpha;

			return newColor;
		}

		/// <summary>
		/// Converts the current color to HSL color space
		/// </summary>
		/// <returns>HSL <see cref="Color2"/></returns>
		public Color2 ToHSL()
		{
			if (_cType == ColorType.HSL)
				return this;

			float h = 0;
			float s = 0;
			float l = 0;
			switch (_cType)
			{
				case ColorType.RGB:
					RGBToHSL(_c1, _c2, _c3, out h, out s, out l);
					break;

				case ColorType.HSV:
					h = _c1;
					HSVToHSL(_c2, _c3, out s, out l);
					break;

				case ColorType.CMYK:
					//Only support RGB <-> CMYK, so convert & return;
					ToRGB().GetHSLf(out h, out s, out l, out _);
					break;
			}

			Color2 newColor = new Color2();
			newColor._cType = ColorType.HSL;
			newColor._c1 = h;
			newColor._c2 = s;
			newColor._c3 = l;
			newColor._alpha = _alpha;

			return newColor;
		}

		/// <summary>
		/// Converts the current color to CMYK color space
		/// </summary>
		/// <returns>CMYK <see cref="Color2"/></returns>
		public Color2 ToCMYK()
		{
			if (_cType == ColorType.CMYK)
				return this;

			float c = 0;
			float m = 0;
			float y = 0;
			float k = 0;
			switch (_cType)
			{
				case ColorType.RGB:
					RGBToCMYK(_c1, _c2, _c3, out c, out m, out y, out k);
					break;

				//Only support RGB <-> CMYK, so convert & return;
				case ColorType.HSV:
				case ColorType.HSL:
					ToRGB().GetCMYKf(out c, out m, out y, out k, out _);
					break;
			}

			Color2 newColor = new Color2();
			newColor._cType = ColorType.CMYK;
			newColor._c1 = c;
			newColor._c2 = m;
			newColor._c3 = y;
			newColor._c4 = k;
			newColor._alpha = _alpha;

			return newColor;
		}


		public bool Equals(Color2 other)
		{
			if (other._cType != _cType)
				return false;

			if (Math.Abs(other._c1 - _c1) > EPSILON)
				return false;

			if (Math.Abs(other._c2 - _c2) > EPSILON)
				return false;

			if (Math.Abs(other._c3 - _c3) > EPSILON)
				return false;

			if (_cType == ColorType.CMYK)
			{
				if (Math.Abs(other._c4 - _c4) > EPSILON)
					return false;
			}

			if (Math.Abs(other._alpha - _alpha) > EPSILON)
				return false;

			return true;
		}

		public override bool Equals(object obj)
		{
			return obj is Color2 ec && ec.Equals(this);
		}

		public override string ToString()
		{
			return $"{ToHexString()}, {ToHTML()}";
		}

		public override int GetHashCode()
		{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			return HashCode.Combine(_cType, _alpha, _c1, _c2, _c3);
#else
			unchecked
			{
				int hash = 17;

				hash = hash * 23 + _cType.GetHashCode();
				hash = hash * 23 + _alpha.GetHashCode();
				hash = hash * 23 + _c1.GetHashCode();
				hash = hash * 23 + _c2.GetHashCode();
				hash = hash * 23 + _c3.GetHashCode();
				return hash;
			}
#endif
		}

		public static bool operator ==(Color2 ec1, Color2 ec2)
		{
			return ec1.Equals(ec2);
		}

		public static bool operator !=(Color2 ec1, Color2 ec2)
		{
			return !ec1.Equals(ec2);
		}

		/// <summary>
		/// Return the equivalent hex string representing the color.
		/// </summary>
		/// <param name="includeAlpha">Whether to include the alpha channel or not</param>
		/// <returns>Hex string of the color</returns>
		public string ToHexString(bool includeAlpha = true)
		{
			GetRGB(out byte r, out byte g, out byte b, out byte a);
			if (includeAlpha)
				return $"#{a:x2}{r:x2}{g:x2}{b:x2}";
			else
				return $"#{r:x2}{g:x2}{b:x2}";
		}

		/// <summary>
		/// Returns the rgb, and a, if specified, of the color in html rgb notation
		/// </summary>
		/// <param name="includeAlpha">Whether to include the alpha channel or not</param>
		/// <returns>HTML formatted rgb(r,g,b) or rgba(r,g,b,a)</returns>
		public string ToHTML(bool includeAlpha = true)
		{
			GetRGB(out byte r, out byte g, out byte b, out byte a);
			if (includeAlpha)
				return $"rgba( {r}, {g}, {b}, {a} )";
			else
				return $"rgb( {r}, {g}, {b} )";
		}

		public string GetDisplayName()
		{
			return KnownColorTable.GetColorName(this);
		}

		public static Color2 FromDisplayName(string name)
		{
			return KnownColorTable.FromColorName(name);
		}

		/// <summary>
		/// Parses the string representing a Hex value or HTML notation to a color. If parsing fails
		/// <see cref="Color2.Empty"/> is returned
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Color2 Parse(string value)
		{
			if (TryParse(value.AsSpan(), out Color2 ec))
				return ec;

			return Empty;
		}

		/// <summary>
		/// Attempts to parse a string as a ReadOnlySpan<char> into a color
		/// </summary>
		/// <param name="value">Value to parse</param>
		/// <param name="ec">The color, if successful</param>
		/// <returns>True if successful, otherwise false</returns>
		public static bool TryParse(ReadOnlySpan<char> value, out Color2 ec)
		{
#if NETSTANDARD2_0
			return TryParseNetStandard2(value, out ec);
#else
			if (value.Contains("#", StringComparison.Ordinal))
			{
				var v = value.Slice(1);
				if (v.Length == 3 || v.Length == 4)
				{
					Span<char> normal = stackalloc char[v.Length * 2];

					for (int i = 0; i < v.Length; i++)
					{
						normal[i * 2] = normal[i * 2 + 1] = v[i];
					}

					if (uint.TryParse(normal, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result))
					{
						ec = FromUInt(result | (v.Length == 3 ? 0xff000000 : 0u));
						return true;
					}
				}
				else if (v.Length == 6 || v.Length == 8)
				{
					if (uint.TryParse(v, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result))
					{
						ec = FromUInt(result | (v.Length == 6 ? 0xff000000 : 0u));
						return true;
					}
				}
			}
			else if (value.StartsWith("rgb"))
			{
				var start = value.IndexOf("(") + 1;
				var result = value.Slice(start, value.Length - start - 1).Trim().ToString().Split(',');

				if (result.Length != 3 && result.Length != 4)
				{
					ec = Empty;
					return false;
				}

				if (byte.TryParse(result[0].Trim(), out byte r) &&
					byte.TryParse(result[1].Trim(), out byte g) &&
					byte.TryParse(result[2].Trim(), out byte b))
				{
					if (result.Length == 4 && byte.TryParse(result[3], out byte a))
					{
						ec = FromARGB(a, r, g, b);
						return true;
					}

					ec = FromRGB(r, g, b);
					return true;
				}
			}

			ec = Empty;
			return false;
#endif
		}

#if NETSTANDARD2_0
		private static bool TryParseNetStandard2(ReadOnlySpan<char> value, out Color2 ec)
		{
			if (value.Contains("#".AsSpan(), StringComparison.Ordinal))
			{
				var v = value.Slice(1);
				if (v.Length == 3 || v.Length == 4)
				{
					Span<char> normal = stackalloc char[v.Length * 2];

					for (int i = 0; i < v.Length; i++)
					{
						normal[i * 2] = normal[i * 2 + 1] = v[i];
					}

					if (uint.TryParse(normal.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result))
					{
						ec = FromUInt(result | (v.Length == 3 ? 0xff000000 : 0u));
						return true;
					}
				}
				else if (v.Length == 6 || v.Length == 8)
				{
					if (uint.TryParse(v.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result))
					{
						ec = FromUInt(result | (v.Length == 6 ? 0xff000000 : 0u));
						return true;
					}
				}
			}
			else if (value.StartsWith("rgb".AsSpan()))
			{
				var start = value.ToString().IndexOf("(") + 1;
				var result = value.Slice(start, value.Length - start - 1).Trim().ToString().Split(',');

				if (result.Length != 3 && result.Length != 4)
				{
					ec = Empty;
					return false;
				}

				if (byte.TryParse(result[0].Trim(), out byte r) &&
					byte.TryParse(result[1].Trim(), out byte g) &&
					byte.TryParse(result[2].Trim(), out byte b))
				{
					if (result.Length == 4 && byte.TryParse(result[3], out byte a))
					{
						ec = FromARGB(a, r, g, b);
						return true;
					}

					ec = FromRGB(r, g, b);
					return true;
				}
			}

			ec = Empty;
			return false;
		}
#endif

		public static implicit operator Color(Color2 ec)
		{
			ec.GetRGB(out byte r, out byte g, out byte b, out byte a);
			return Color.FromArgb(a, r, g, b);
		}

		public static implicit operator Color2(Color c)
		{
			return FromUInt(c.ToUint32());
		}

		public static bool AreColorsClose(Color2 col1, Color2 col2, float tolerance = 0.03f)
		{
			return false;
			//switch (col1._cType)
			//{
			//	case ColorType.RGB:
			//		{
			//			if (col2._cType != ColorType.RGB)
			//				col2 = col2.ToRGB();

			//			if (MathF.Abs(col2._c1 - col1._c1) < tolerance)
			//				return true;

			//			if (MathF.Abs(col2._c2 - col1._c2) < tolerance)
			//				return true;

			//			if (MathF.Abs(col2._c3 - col1._c3) < tolerance)
			//				return true;
			//		}
			//		break;

			//	case ColorType.HSV:

			//		break;

			//	case ColorType.HSL:

			//		break;

			//	case ColorType.CMYK:

			//		break;

			//	default:
			//		return false;
			//}
		}

		/// <summary>
		/// Creates an RGB <see cref="Color2"/> from the specified R,G,B values. The alpha is set to 255
		/// </summary>
		/// <param name="r">Red, [0,255]</param>
		/// <param name="g">Green, [0,255]</param>
		/// <param name="b">Blue, [0,255]</param>
		/// <returns>RGB <see cref="Color2"/></returns>
		public static Color2 FromRGB(byte r, byte g, byte b)
		{
			return new Color2(r, g, b);
		}

		/// <summary>
		/// Creates an RGB <see cref="Color2"/> from the specified A,R,G,B values.
		/// </summary>
		/// <param name="a">Alpha, [0,255]</param>
		/// <param name="r">Red, [0,255]</param>
		/// <param name="g">Green, [0,255]</param>
		/// <param name="b">Blue, [0,255]</param>
		/// <returns>RGB <see cref="Color2"/></returns>
		public static Color2 FromARGB(byte a, byte r, byte g, byte b)
		{
			return new Color2(r, g, b, a);
		}

		/// <summary>
		/// Creates an RGB <see cref="Color2"/> from the specified A,R,G,B float values.
		/// </summary>
		/// <param name="r">Red, [0,1]</param>
		/// <param name="g">Green, [0,1]</param>
		/// <param name="b">Blue, [0,1]</param>
		/// /// <param name="a">Alpha, [0,1]</param>
		/// <returns>RGB <see cref="Color2"/></returns>
		public static Color2 FromRGBf(float r, float g, float b, float a = 1)
		{
			Color2 newColor = new Color2();
			newColor._cType = ColorType.RGB;
			newColor._c1 = MathHelpers.Clamp(r, 0, 1);
			newColor._c2 = MathHelpers.Clamp(g, 0, 1);
			newColor._c3 = MathHelpers.Clamp(b, 0, 1);
			newColor._alpha = MathHelpers.Clamp(a, 0, 1);
			return newColor;
		}

		/// <summary>
		/// Creates an RGB <see cref="Color2"/> from an unsigned integer
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		public static Color2 FromUInt(uint num)
		{
			byte a = (byte)((num >> 24) & 0xFF);
			byte r = (byte)((num >> 16) & 0xFF);
			byte g = (byte)((num >> 8) & 0xFF);
			byte b = (byte)(num & 0xFF);

			return new Color2(r, g, b, a);
		}

		/// <summary>
		/// Creates an HSV <see cref="Color2"/> from the given values
		/// </summary>
		/// <param name="hue">Hue, [0,360)</param>
		/// <param name="sat">Saturation, [0,100]</param>
		/// <param name="val">Value, [0,100]</param>
		/// <param name="alpha">Alpha, [0,255]</param>
		/// <returns>HSV <see cref="Color2"/></returns>
		public static Color2 FromHSV(int hue, int sat, int val, int alpha = 255)
		{
			return FromHSVf(hue, sat / 100f, val / 100f, alpha / 255f);
		}

		/// <summary>
		/// Creates an HSV <see cref="Color2"/> from the given float values
		/// </summary>
		/// <param name="hue">Hue, [0,360)</param>
		/// <param name="sat">Saturation, [0,1]</param>
		/// <param name="val">Value, [0,1]</param>
		/// <param name="alpha">Alpha, [0,1]</param>
		/// <returns>HSV <see cref="Color2"/></returns>
		public static Color2 FromHSVf(float hue, float sat, float val, float alpha = 1)
		{
			Color2 newColor = new Color2();
			newColor._cType = ColorType.HSV;
			newColor._c1 = hue == -1 ? 0 : MathHelpers.Clamp(hue % 360, 0, 360);
			newColor._c2 = MathHelpers.Clamp(sat, 0, 1);
			newColor._c3 = MathHelpers.Clamp(val, 0, 1);
			newColor._alpha = MathHelpers.Clamp(alpha, 0, 1);
			return newColor;
		}

		/// <summary>
		/// Creates an HSL <see cref="Color2"/> from the given values
		/// </summary>
		/// <param name="hue">Hue, [0,360)</param>
		/// <param name="sat">Saturation, [0,100]</param>
		/// <param name="light">Value, [0,100]</param>
		/// <param name="alpha">Alpha, [0,255]</param>
		/// <returns>HSL <see cref="Color2"/></returns>
		public static Color2 FromHSL(int hue, int sat, int light, int alpha = 255)
		{
			return FromHSLf(hue, sat / 100f, light / 100f, alpha / 255f);
		}

		/// <summary>
		/// Creates an HSL <see cref="Color2"/> from the given float values
		/// </summary>
		/// <param name="hue">Hue, [0,360)</param>
		/// <param name="sat">Saturation, [0,1]</param>
		/// <param name="light">Value, [0,1]</param>
		/// <param name="alpha">Alpha, [0,1]</param>
		/// <returns>HSL <see cref="Color2"/></returns>
		public static Color2 FromHSLf(float hue, float sat, float light, float alpha = 1)
		{
			Color2 newColor = new Color2();
			newColor._cType = ColorType.HSL;
			newColor._c1 = hue == -1 ? 0 : MathHelpers.Clamp(hue % 360, 0, 360);
			newColor._c2 = MathHelpers.Clamp(sat, 0, 1);
			newColor._c3 = MathHelpers.Clamp(light, 0, 1);
			newColor._alpha = MathHelpers.Clamp(alpha, 0, 1);
			return newColor;
		}

		/// <summary>
		/// Creates a CMYK <see cref="Color2"/> from the given values
		/// </summary>
		/// <param name="c">Cyan, [0,100]</param>
		/// <param name="m">Magenta, [0,100]</param>
		/// <param name="y">Yellow, [0,100]</param>
		/// <param name="k">Black, [0,100]</param>
		/// <param name="alpha">Cyan, [0,255]</param>
		/// <returns>CMYK <see cref="Color2"/></returns>
		public static Color2 FromCMYK(int c, int m, int y, int k, int alpha = 255)
		{
			return FromCMYKf(c / 100f, m / 100f, y / 100f, k / 100f, alpha / 255);
		}

		/// <summary>
		/// Creates a CMYK <see cref="Color2"/> from the given float values
		/// </summary>
		/// <param name="c">Cyan, [0,1]</param>
		/// <param name="m">Magenta, [0,1]</param>
		/// <param name="y">Yellow, [0,1]</param>
		/// <param name="k">Black, [0,1]</param>
		/// <param name="alpha">Cyan, [0,1]</param>
		/// <returns>CMYK <see cref="Color2"/></returns>
		public static Color2 FromCMYKf(float c, float m, float y, float k, float alpha = 1)
		{
			Color2 newColor = new Color2();
			newColor._cType = ColorType.CMYK;
			newColor._c1 = MathHelpers.Clamp(c, 0, 1);
			newColor._c2 = MathHelpers.Clamp(m, 0, 1);
			newColor._c3 = MathHelpers.Clamp(y, 0, 1);
			newColor._c4 = MathHelpers.Clamp(k, 0, 1);
			newColor._alpha = MathHelpers.Clamp(alpha, 0, 1);
			return newColor;
		}


		public Color2 WithHue(int h)
		{
			GetHSV(out _, out int s, out int v, out int a);

			return Color2.FromHSV(h, s, v, a);
		}

		public Color2 WithHuef(float h)
		{
			GetHSVf(out _, out float s, out float v, out float a);

			return Color2.FromHSVf(h, s, v, a);
		}

		public Color2 WithSat(int s)
		{
			GetHSV(out int h, out _, out int v, out int a);

			return FromHSV(h, s, v, a);
		}

		public Color2 WithSatf(float s)
		{
			GetHSVf(out float h, out _, out float v, out float a);

			return FromHSVf(h, s, v, a);
		}

		public Color2 WithVal(int v)
		{
			GetHSV(out int h, out int s, out _, out int a);

			return FromHSV(h, s, v, a);
		}

		public Color2 WithValf(float v)
		{
			GetHSVf(out float h, out float s, out _, out float a);

			return FromHSVf(h, s, v, a);
		}

		public Color2 WithRed(int r)
		{
			GetRGB(out _, out byte g, out byte b, out byte a);

			return new Color2((byte)r, g, b, a);
		}

		public Color2 WithRedf(float r)
		{
			GetRGBf(out _, out float g, out float b, out float a);

			return FromRGBf(r, g, b, a);
		}

		public Color2 WithGreen(int g)
		{
			GetRGB(out byte r, out _, out byte b, out byte a);

			return new Color2(r, (byte)g, b, a);
		}

		public Color2 WithGreenf(float g)
		{
			GetRGBf(out float r, out _, out float b, out float a);

			return FromRGBf(r, g, b, a);
		}

		public Color2 WithBlue(int b)
		{
			GetRGB(out byte r, out byte g, out _, out byte a);

			return new Color2(r, g, (byte)b, a);
		}

		public Color2 WithBluef(float b)
		{
			GetRGBf(out float r, out float g, out _, out float a);

			return FromRGBf(r, g, b, a);
		}

		public Color2 WithAlpha(int a)
		{
			return WithAlphaf(a / 255f);
		}

		public Color2 WithAlphaf(float a)
		{
			switch (_cType)
			{
				case ColorType.RGB:
					return FromRGBf(Rf, Gf, Bf, a);

				case ColorType.HSV:
					return FromHSVf(Huef, Saturationf, Valuef, a);

				case ColorType.HSL:
					return FromHSLf(Huef, HSLSaturationf, Lightnessf, a);

				case ColorType.CMYK:
					return FromCMYKf(CMYKCyanf, CMYKMagentaf, CMYKYellowf, CMYKBlackf, a);

				default:
					return Empty;
			}
		}

		/// <summary>
		/// Lightens or darkens a color by a specified lightness, converting to an HSL color if necessary
		/// </summary>
		/// <param name="amount">Amount to lighten/darken</param>
		/// <returns>HSL Color2 with the new lightness (old + amount)</returns>
		public Color2 Lighten(float amount)
		{
			if (_cType != ColorType.HSL)
				return ToHSL().Lighten(amount);

			var l = _c3 + amount;
			MathHelpers.Clamp(l, 0, 1);

			return FromHSLf(_c1, _c2, l, _alpha);
		}

		/// <summary>
		/// Lightens or darkens a color by a percentage of the current lightness
		/// </summary>
		/// <param name="percent"></param>
		/// <returns>HSL Color2 with the new lightness (old + (old * percent))</returns>
		public Color2 LightenPercent(float percent)
		{
			if (_cType != ColorType.HSL)
				return ToHSL().LightenPercent(percent);

			var l = _c3 < EPSILON ? percent : _c3 + (_c3 * percent);
			MathHelpers.Clamp(l, 0, 1);

			return FromHSLf(_c1, _c2, l, _alpha);
		}


		public static void HSVToRGB(float hue, float sat, float val, out float r, out float g, out float b)
		{
			r = g = b = val;
			
			if (hue >= 0 && sat >= EPSILON)
			{
				hue = (hue / 360f) * 6f;
				
				var h = (int)hue;
				var v1 = val * (1f - sat);
				var v2 = val * (1f - sat * (hue - h));
				var v3 = val * (1f - sat * (1f - (hue - h)));

				switch (h)
				{
					case 0:
						r = val;
						g = v3;
						b = v1;
						break;

					case 1:
						r = v2;
						g = val;
						b = v1;
						break;

					case 2:
						r = v1;
						g = val;
						b = v3;
						break;

					case 3:
						r = v1;
						g = v2;
						b = val;
						break;

					case 4:
						r = v3;
						g = v1;
						b = val;
						break;

					case 5:
						r = val;
						g = v1;
						b = v2;
						break;
				}
			}
		}

		public static void RGBToHSV(float r, float g, float b, out float h, out float s, out float v)
		{
			var min = MathF.Min(r, MathF.Min(g, b));
			var max = MathF.Max(r, MathF.Max(g, b));
			var delta = max - min;

			h = 0;
			s = 0;
			v = max;

			if (delta > EPSILON)
			{
				s = delta / max;

				if (MathF.Abs(r-max) < EPSILON)
				{
					h = ((g - b) / delta);
				}
				else if (MathF.Abs(g - max) < EPSILON)
				{
					h = (2f + (b - r) / delta);
				}
				else
				{
					h = (4f + (r - g) / delta);
				}

				h *= 60;
			}

			if (h < 0)
				h += 360;
			else if (h >= 360)
				h -= 360;
		}

		public static void HSLToRGB(float h, float s, float l, out float r, out float g, out float b)
		{
			h /= 360f;

			r = l;
			g = l;
			b = l;

			//Adapted from SkiaSharp
			if (s > EPSILON)
			{
				float v2;
				if (l < 0.5f)
					v2 = l * (1 + s);
				else
					v2 = (l + s) - (s * l);

				var v1 = 2f * l - v2;

				static float HueToRGB(float v1, float v2, float vH)
				{
					if (vH < 0)
						vH += 1f;
					if (vH > 1)
						vH -= 1f;

					if ((6f * vH) < 1f)
						return (v1 + (v2 - v1) * 6f * vH);
					if ((2f * vH) < 1f)
						return (v2);
					if ((3f * vH) < 2f)
						return (v1 + (v2 - v1) * ((2f / 3f) - vH) * 6f);
					return (v1);
				}

				r = HueToRGB(v1, v2, h + (1f / 3f));
				g = HueToRGB(v1, v2, h);
				b = HueToRGB(v1, v2, h - (1f / 3f));
			}
		}

		public static void RGBToHSL(float r, float g, float b, out float h, out float s, out float l)
		{
			var min = MathF.Min(r, MathF.Min(g, b));
			var max = MathF.Max(r, MathF.Max(g, b));
			var delta = max - min;

			h = 0;
			s = 0;
			l = (max + min) * 0.5f;

			if (delta > EPSILON)
			{
				if (l < 0.5)
					s = delta / (max + min);
				else
					s = delta / (2f - max - min);

				if (MathF.Abs(r - max) < EPSILON)
					h = ((g - b) / delta);
				else if (MathF.Abs(g - max) < EPSILON)
					h = (2 + (b - r) / delta);
				else if (MathF.Abs(b - max) < EPSILON)
					h = (4 + (r - g) / delta);

				h *= 60;
				if (h < 0)
					h += 360;
				else if (h >= 360)
					h -= 360;
			}
		}

		public static void HSVToHSL(float hsvSat, float val, out float hslSat, out float l)
		{
			l = val * (1 - hsvSat / 2f);
			hslSat = 0;

			if (l > EPSILON && MathF.Abs(1 - l) > EPSILON)
			{
				hslSat = 2 * (1 - (l / val));
			}
		}

		public static void HSLToHSV(float hslSat, float l, out float hsvSat, out float v)
		{
			v = l + hslSat * MathF.Min(l, 1 - l);
			hsvSat = v < EPSILON ? 0 : (2 * (1 - l / v));
		}

		public static void RGBToCMYK(float r, float g, float b, out float c, out float m, out float y, out float k)
		{
			c = 1 - r;
			m = 1 - g;
			y = 1 - b;
			k = MathF.Min(c, MathF.Min(m, y));

			c = (c - k) / (1 - k);
			m = (m - k) / (1 - k);
			y = (y - k) / (1 - k);
		}

		public static void CMYKToRGB(float c, float m, float y, float k, out float r, out float g, out float b)
		{
			r = (1 - c) * (1 - k);
			g = (1 - m) * (1 - k);
			b = (1 - y) * (1 - k);
		}

		public static void HSVToUInt(float hue, float sat, float val, out uint num)
		{
			HSVToRGB(hue, sat, val, out float r, out float g, out float b);
			
			num = ((uint)0xFF << 24) | ((uint)(r*255) << 16) | ((uint)(g*255) << 8) | (uint)(b*255);
		}

		private const float EPSILON = 0.001f;
        private ColorType _cType;
        private float _alpha;
        private float _c1;//Red, Hue, or CMYK 'C'
        private float _c2;//Green, Saturation, or CMYK 'M'
        private float _c3;//Blue, Value, Lightness, or CMYK 'Y'
		private float _c4;//CMYK 'k' only, unused otherwise
        public static readonly Color2 Empty = new Color2();
    }

	internal static class KnownColorTable
	{
		private static Dictionary<Color2, string> ColorTable;

		public static string GetColorName(Color2 c)
		{
			InitColorTable();

			if (ColorTable.TryGetValue(c, out string value))
			{
				return value;
			}

			return "";
		}

		public static Color2 FromColorName(string name)
		{
			foreach(var kvp in ColorTable)
			{
				if (name.Equals(kvp.Value, StringComparison.OrdinalIgnoreCase))
					return kvp.Key;
			}

			return Color2.Empty;
		}

		private static void InitColorTable()
		{
			if (ColorTable == null)
			{
				var kcs = (KnownColor[])Enum.GetValues(typeof(KnownColor));
				var names = Enum.GetNames(typeof(KnownColor));

				ColorTable = new Dictionary<Color2, string>(kcs.Length);
				for (int i = 0; i < kcs.Length; i++)
				{
					var c2 = Color2.FromUInt((uint)kcs[i]);
					if (!ColorTable.ContainsKey(c2))
						ColorTable.Add(c2, names[i]);
				}
			}
		}
	}

	internal enum KnownColor : uint
	{
		None,
		AliceBlue = 0xfff0f8ff,
		AntiqueWhite = 0xfffaebd7,
		Aqua = 0xff00ffff,
		Aquamarine = 0xff7fffd4,
		Azure = 0xfff0ffff,
		Beige = 0xfff5f5dc,
		Bisque = 0xffffe4c4,
		Black = 0xff000000,
		BlanchedAlmond = 0xffffebcd,
		Blue = 0xff0000ff,
		BlueViolet = 0xff8a2be2,
		Brown = 0xffa52a2a,
		BurlyWood = 0xffdeb887,
		CadetBlue = 0xff5f9ea0,
		Chartreuse = 0xff7fff00,
		Chocolate = 0xffd2691e,
		Coral = 0xffff7f50,
		CornflowerBlue = 0xff6495ed,
		Cornsilk = 0xfffff8dc,
		Crimson = 0xffdc143c,
		Cyan = 0xff00ffff,
		DarkBlue = 0xff00008b,
		DarkCyan = 0xff008b8b,
		DarkGoldenrod = 0xffb8860b,
		DarkGray = 0xffa9a9a9,
		DarkGreen = 0xff006400,
		DarkKhaki = 0xffbdb76b,
		DarkMagenta = 0xff8b008b,
		DarkOliveGreen = 0xff556b2f,
		DarkOrange = 0xffff8c00,
		DarkOrchid = 0xff9932cc,
		DarkRed = 0xff8b0000,
		DarkSalmon = 0xffe9967a,
		DarkSeaGreen = 0xff8fbc8f,
		DarkSlateBlue = 0xff483d8b,
		DarkSlateGray = 0xff2f4f4f,
		DarkTurquoise = 0xff00ced1,
		DarkViolet = 0xff9400d3,
		DeepPink = 0xffff1493,
		DeepSkyBlue = 0xff00bfff,
		DimGray = 0xff696969,
		DodgerBlue = 0xff1e90ff,
		Firebrick = 0xffb22222,
		FloralWhite = 0xfffffaf0,
		ForestGreen = 0xff228b22,
		Fuchsia = 0xffff00ff,
		Gainsboro = 0xffdcdcdc,
		GhostWhite = 0xfff8f8ff,
		Gold = 0xffffd700,
		Goldenrod = 0xffdaa520,
		Gray = 0xff808080,
		Green = 0xff008000,
		GreenYellow = 0xffadff2f,
		Honeydew = 0xfff0fff0,
		HotPink = 0xffff69b4,
		IndianRed = 0xffcd5c5c,
		Indigo = 0xff4b0082,
		Ivory = 0xfffffff0,
		Khaki = 0xfff0e68c,
		Lavender = 0xffe6e6fa,
		LavenderBlush = 0xfffff0f5,
		LawnGreen = 0xff7cfc00,
		LemonChiffon = 0xfffffacd,
		LightBlue = 0xffadd8e6,
		LightCoral = 0xfff08080,
		LightCyan = 0xffe0ffff,
		LightGoldenrodYellow = 0xfffafad2,
		LightGreen = 0xff90ee90,
		LightGray = 0xffd3d3d3,
		LightPink = 0xffffb6c1,
		LightSalmon = 0xffffa07a,
		LightSeaGreen = 0xff20b2aa,
		LightSkyBlue = 0xff87cefa,
		LightSlateGray = 0xff778899,
		LightSteelBlue = 0xffb0c4de,
		LightYellow = 0xffffffe0,
		Lime = 0xff00ff00,
		LimeGreen = 0xff32cd32,
		Linen = 0xfffaf0e6,
		Magenta = 0xffff00ff,
		Maroon = 0xff800000,
		MediumAquamarine = 0xff66cdaa,
		MediumBlue = 0xff0000cd,
		MediumOrchid = 0xffba55d3,
		MediumPurple = 0xff9370db,
		MediumSeaGreen = 0xff3cb371,
		MediumSlateBlue = 0xff7b68ee,
		MediumSpringGreen = 0xff00fa9a,
		MediumTurquoise = 0xff48d1cc,
		MediumVioletRed = 0xffc71585,
		MidnightBlue = 0xff191970,
		MintCream = 0xfff5fffa,
		MistyRose = 0xffffe4e1,
		Moccasin = 0xffffe4b5,
		NavajoWhite = 0xffffdead,
		Navy = 0xff000080,
		OldLace = 0xfffdf5e6,
		Olive = 0xff808000,
		OliveDrab = 0xff6b8e23,
		Orange = 0xffffa500,
		OrangeRed = 0xffff4500,
		Orchid = 0xffda70d6,
		PaleGoldenrod = 0xffeee8aa,
		PaleGreen = 0xff98fb98,
		PaleTurquoise = 0xffafeeee,
		PaleVioletRed = 0xffdb7093,
		PapayaWhip = 0xffffefd5,
		PeachPuff = 0xffffdab9,
		Peru = 0xffcd853f,
		Pink = 0xffffc0cb,
		Plum = 0xffdda0dd,
		PowderBlue = 0xffb0e0e6,
		Purple = 0xff800080,
		Red = 0xffff0000,
		RosyBrown = 0xffbc8f8f,
		RoyalBlue = 0xff4169e1,
		SaddleBrown = 0xff8b4513,
		Salmon = 0xfffa8072,
		SandyBrown = 0xfff4a460,
		SeaGreen = 0xff2e8b57,
		SeaShell = 0xfffff5ee,
		Sienna = 0xffa0522d,
		Silver = 0xffc0c0c0,
		SkyBlue = 0xff87ceeb,
		SlateBlue = 0xff6a5acd,
		SlateGray = 0xff708090,
		Snow = 0xfffffafa,
		SpringGreen = 0xff00ff7f,
		SteelBlue = 0xff4682b4,
		Tan = 0xffd2b48c,
		Teal = 0xff008080,
		Thistle = 0xffd8bfd8,
		Tomato = 0xffff6347,
		Transparent = 0x00ffffff,
		Turquoise = 0xff40e0d0,
		Violet = 0xffee82ee,
		Wheat = 0xfff5deb3,
		White = 0xffffffff,
		WhiteSmoke = 0xfff5f5f5,
		Yellow = 0xffffff00,
		YellowGreen = 0xff9acd32
	}

	public class Color2ToColorConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(Color);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(Color);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is Color c)
            {
                return Color2.FromUInt(c.ToUint32());
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is Color2 c)
            {
                c.GetRGB(out byte r, out byte g, out byte b, out byte a);
                return new Color(a, r, g, b);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
