using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Text;

namespace FluentAvalonia.UI.Media
{
    internal enum ColorType
    {
        Undefined,
        RGB,
        HSV,
        HSL
    }

    [TypeConverter(typeof(Color2ToColorConverter))]
    public struct Color2 : IEquatable<Color2>
    {
        /// <summary>
        /// Creates a RGB Color2
        /// </summary>
        /// <param name="red">Red, range 0 - 255</param>
        /// <param name="green">Green, range 0 - 255</param>
        /// <param name="blue">Blue, range 0 - 255</param>
        /// <param name="alpha">Alpha, range 0 - 255</param>
        /// <returns></returns>
        public static Color2 FromRGB(int red, int green, int blue, int alpha = 255)
        {
            if (red < 0 || red > 255 || green < 0 || green > 255 || blue < 0 || blue > 255 ||
                alpha < 0 || alpha > 255)
                throw new ArgumentOutOfRangeException();

            Color2 ec = new Color2();
            ec._cType = ColorType.RGB;
            ec._c1 = red / 255f;
            ec._c2 = green / 255f;
            ec._c3 = blue / 255f;
            ec._alpha = alpha / 255f;
            return ec;
        }

        /// <summary>
        /// Creates a RGB Color2 using floating point numbers
        /// </summary>
        /// <param name="red">Red, range 0 - 1</param>
        /// <param name="green">Green, range 0 - 1</param>
        /// <param name="blue">Blue, range 0 - 1</param>
        /// <param name="alpha">Alpha, range 0 - 1</param>
        /// <returns></returns>
        public static Color2 FromRGB(float red, float green, float blue, float alpha = 1.0f)
        {
            if (red < 0 || red > 1 || green < 0 || green > 1 || blue < 0 || blue > 1 ||
                alpha < 0 || alpha > 1)
                throw new ArgumentOutOfRangeException();

            Color2 ec = new Color2();
            ec._cType = ColorType.RGB;
            ec._c1 = red;
            ec._c2 = green;
            ec._c3 = blue;
            ec._alpha = alpha;
            return ec;
        }

        /// <summary>
        /// Creates an HSV Color2
        /// </summary>
        /// <param name="hue">Hue, Must be > 0, generally [0,360)</param>
        /// <param name="sat">HSV Saturation, range 0 - 255</param>
        /// <param name="val">HSV Value, range 0 - 255</param>
        /// <param name="alpha">Alpha, 0 - 255</param>
        /// <returns></returns>
        public static Color2 FromHSV(int hue, int sat, int val, int alpha = 255)
        {
            if (hue < -1 || sat < 0 || sat > 255 || val < 0 || val > 255 || alpha < 0 || alpha > 255)
                throw new ArgumentOutOfRangeException();

            Color2 ec = new Color2();
            ec._cType = ColorType.HSV;
            ec._c1 = hue == -1 ? 0 : hue % 360;
            ec._c2 = sat / 255f;
            ec._c3 = val / 255f;
            ec._alpha = alpha / 255f;
            return ec;
        }

        /// <summary>
        /// Creates a HSV Color2 using floating point numbers
        /// </summary>
        /// <param name="hue">Hue, must be > 0, generally [0,360)</param>
        /// <param name="sat">HSV Saturation, range 0 - 1</param>
        /// <param name="val">HSV Value, range 0 - 1</param>
        /// <param name="alpha">Alpha, range 0 - 1</param>
        /// <returns></returns>
        public static Color2 FromHSV(float hue, float sat, float val, float alpha = 1.0f)
        {
            if (hue < -1 || sat < 0 || sat > 1 || val < 0 || val > 1 || alpha < 0 || alpha > 1)
                throw new ArgumentOutOfRangeException();

            Color2 ec = new Color2();
            ec._cType = ColorType.HSV;
            ec._c1 = hue == -1 ? 0 : hue % 360;
            ec._c2 = sat;
            ec._c3 = val;
            ec._alpha = alpha;
            return ec;
        }


        public static Color2 FromUInt32(uint value)
        {
            byte r = (byte)((value >> 16) & 0xFF);
            byte g = (byte)((value >> 8) & 0xFF);
            byte b = (byte)((value) & 0xFF);
            byte a = (byte)((value >> 24) & 0xFF);

            return Color2.FromRGB(r, g, b, a);
        }

        /// <summary>
        /// Gets or sets the alpha of the color
        /// </summary>
        public byte A
        {
            get => (byte)Math.Round(_alpha * 255);
            set => _alpha = value / 255f;
        }

        /// <summary>
        /// Gets or sets the Red channel of the color. Note: setting this value
        /// will convert the color to RGB if not already in it
        /// </summary>
        public byte R
        {
            get
            {
                if (_cType != ColorType.RGB)
                {
                    ToRGB(out float r, out float _, out float _);
                    return (byte)Math.Round(r * 255);
                }

                return (byte)Math.Round(_c1 * 255);
            }
            set
            {
                if (_cType == ColorType.RGB)
                {
                    _c1 = value / 255f;
                    return;
                }

                ToRGB(out float r, out float g, out float b);
                _cType = ColorType.RGB;
                _c1 = value / 255f;
                _c2 = g;
                _c3 = b;
            }
        }

        /// <summary>
        /// Gets or sets the Green channel of the color. Note: setting this value
        /// will convert the color to RGB if not already in it
        /// </summary>
        public byte G
        {
            get
            {
                if (_cType != ColorType.RGB)
                {
                    ToRGB(out float _, out float g, out float _);
                    return (byte)Math.Round(g * 255);
                }

                return (byte)Math.Round(_c2 * 255);
            }
            set
            {
                if (_cType == ColorType.RGB)
                {
                    _c2 = value / 255f;
                    return;
                }

                ToRGB(out float r, out float g, out float b);
                _cType = ColorType.RGB;
                _c1 = r;
                _c2 = value / 255f;
                _c3 = b;
            }
        }

        /// <summary>
        /// Gets or sets the Blue channel of the color. Note: setting this value
        /// will convert the color to RGB if not already in it
        /// </summary>
        public byte B
        {
            get
            {
                if (_cType != ColorType.RGB)
                {
                    ToRGB(out float _, out float _, out float b);
                    return (byte)Math.Round(b * 255);
                }

                return (byte)Math.Round(_c3 * 255);
            }
            set
            {
                if (_cType == ColorType.RGB)
                {
                    _c3 = value / 255f;
                    return;
                }

                ToRGB(out float r, out float g, out float b);
                _cType = ColorType.RGB;
                _c1 = r;
                _c2 = g;
                _c3 = value / 255f;
            }
        }

        /// <summary>
        /// Gets or sets the Hue of the color. Note: setting this value
        /// will convert the color to HSV if not already in it
        /// </summary>
        public int Hue
        {
            get
            {
                if (_cType != ColorType.HSV)
                {
                    ToHSV(out float hue, out float _, out float _);
                    return (int)Math.Round(hue);
                }

                return (int)Math.Round(_c1);
            }
            set
            {
                if (_cType == ColorType.HSV)
                {
                    _c1 = value;
                    return;
                }

                ToHSV(out float h, out float s, out float v);
                _cType = ColorType.HSV;
                _c1 = value;
                _c2 = s;
                _c3 = v;
            }
        }

        /// <summary>
        /// Gets or sets the Saturation of the color. Note: setting this value
        /// will convert the color to HSV if not already in it
        /// </summary>
        public int Saturation
        {
            get
            {
                if (_cType != ColorType.HSV)
                {
                    ToHSV(out float _, out float sat, out float _);
                    return (int)Math.Round(sat * 255);
                }

                return (int)Math.Round(_c2 * 255);
            }
            set
            {
                if (_cType == ColorType.HSV)
                {
                    _c2 = value / 255f;
                    return;
                }

                ToHSV(out float h, out float s, out float v);
                _cType = ColorType.HSV;
                _c1 = h;
                _c2 = value / 255f;
                _c3 = v;
            }
        }

        /// <summary>
        /// Gets or sets the Value of the color. Note: setting this value
        /// will convert the color to HSV if not already in it
        /// </summary>
        public int Value
        {
            get
            {
                if (_cType != ColorType.HSV)
                {
                    ToHSV(out float _, out float _, out float val);
                    return (int)Math.Round(val * 255);
                }

                return (int)Math.Round(_c3 * 255);
            }
            set
            {
                if (_cType == ColorType.HSV)
                {
                    _c3 = value / 255f;
                    return;
                }

                ToHSV(out float h, out float s, out float v);
                _cType = ColorType.HSV;
                _c1 = h;
                _c2 = s;
                _c3 = value / 255f;
            }
        }


        /// <summary>
        /// Gets or sets the Alpha of the color as a floating point number.
        /// </summary>
        public float Af
        {
            get => _alpha;
            set => _alpha = value;
        }

        /// <summary>
        /// Gets or sets the Red of the color as a floating point number.
        /// Note: setting this value will convert the color to RGB if not already in it
        /// </summary>
        public float Rf
        {
            get
            {
                if (_cType != ColorType.RGB)
                {
                    ToRGB(out float r, out float _, out float _);
                    return r;
                }

                return _c1;
            }
            set
            {
                if (_cType == ColorType.RGB)
                {
                    _c1 = value;
                    return;
                }

                ToRGB(out float r, out float g, out float b);
                _cType = ColorType.RGB;
                _c1 = value;
                _c2 = g;
                _c3 = b;
            }
        }

        /// <summary>
        /// Gets or sets the Green of the color as a floating point number.
        /// Note: setting this value will convert the color to RGB if not already in it
        /// </summary>
        public float Gf
        {
            get
            {
                if (_cType != ColorType.RGB)
                {
                    ToRGB(out float _, out float g, out float _);
                    return g;
                }

                return _c2;
            }
            set
            {
                if (_cType == ColorType.RGB)
                {
                    _c2 = value;
                    return;
                }

                ToRGB(out float r, out float g, out float b);
                _cType = ColorType.RGB;
                _c1 = r;
                _c2 = value;
                _c3 = b;
            }
        }

        /// <summary>
        /// Gets or sets the Blue of the color as a floating point number.
        /// Note: setting this value will convert the color to RGB if not already in it
        /// </summary>
        public float Bf
        {
            get
            {
                if (_cType != ColorType.RGB)
                {
                    ToRGB(out float _, out float _, out float b);
                    return b;
                }

                return _c3;
            }
            set
            {
                if (_cType == ColorType.RGB)
                {
                    _c3 = value;
                    return;
                }

                ToRGB(out float r, out float g, out float b);
                _cType = ColorType.RGB;
                _c1 = r;
                _c2 = g;
                _c3 = value;
            }
        }

        /// <summary>
        /// Gets or sets the Hue of the color as a floating point number.
        /// Note: setting this value will convert the color to HSV if not already in it
        /// </summary>
        public float Huef
        {
            get
            {
                if (_cType != ColorType.HSV)
                {
                    ToHSV(out float h, out float _, out float _);
                    return h;
                }

                return _c1;
            }
            set
            {
                if (_cType == ColorType.HSV)
                {
                    _c1 = value;
                    return;
                }

                ToHSV(out float h, out float s, out float v);
                _cType = ColorType.HSV;
                _c1 = value;
                _c2 = s;
                _c3 = v;
            }
        }

        /// <summary>
        /// Gets or sets the Saturation of the color as a floating point number.
        /// Note: setting this value will convert the color to HSV if not already in it
        /// </summary>
        public float Saturationf
        {
            get
            {
                if (_cType != ColorType.HSV)
                {
                    ToHSV(out float _, out float s, out float _);
                    return s;
                }

                return _c2;
            }
            set
            {
                if (_cType == ColorType.HSV)
                {
                    _c2 = value;
                    return;
                }

                ToHSV(out float h, out float s, out float v);
                _cType = ColorType.HSV;
                _c1 = h;
                _c2 = value;
                _c3 = v;
            }
        }

        /// <summary>
        /// Gets or sets the Value of the color as a floating point number.
        /// Note: setting this value will convert the color to HSV if not already in it
        /// </summary>
        public float Valuef
        {
            get
            {
                if (_cType != ColorType.HSV)
                {
                    ToHSV(out float _, out float _, out float v);
                    return v;
                }

                return _c3;
            }
            set
            {
                if (_cType == ColorType.HSV)
                {
                    _c3 = value;
                    return;
                }

                ToHSV(out float h, out float s, out float v);
                _cType = ColorType.HSV;
                _c1 = h;
                _c2 = s;
                _c3 = value;
            }
        }


        /// <summary>
        /// Gets all ARGB components at once
        /// </summary>
        public void GetARGB(out byte red, out byte green, out byte blue, out byte alpha)
        {
            alpha = (byte)Math.Round(_alpha * 255);
            if (_cType != ColorType.RGB)
            {
                ToRGB(out float r, out float g, out float b);
                red = (byte)Math.Round(r * 255);
                green = (byte)Math.Round(g * 255);
                blue = (byte)Math.Round(b * 255);
                return;
            }
            red = (byte)Math.Round(_c1 * 255);
            green = (byte)Math.Round(_c2 * 255);
            blue = (byte)Math.Round(_c3 * 255);
        }

        /// <summary>
        /// Gets all ARGB components at once as floating point numbers
        /// </summary>
        public void GetARGBf(out float red, out float green, out float blue, out float alpha)
        {
            alpha = _alpha;
            if (_cType != ColorType.RGB)
            {
                ToRGB(out float r, out float g, out float b);
                red = r;
                green = g;
                blue = b;
                return;
            }
            red = _c1;
            green = _c2;
            blue = _c3;
        }


        /// <summary>
        /// Gets all AHSV components at once
        /// </summary>
        public void GetAHSV(out int hue, out int saturation, out int value, out int alpha)
        {
            alpha = (int)Math.Round(_alpha * 255);
            if (_cType != ColorType.HSV)
            {
                ToHSV(out float f, out float s, out float v);
                hue = (int)Math.Round(f);
                saturation = (int)Math.Round(s * 255);
                value = (int)Math.Round(v * 255);
                return;
            }
            hue = (int)Math.Round(_c1);
            saturation = (int)Math.Round(_c2 * 255);
            value = (int)Math.Round(_c3 * 255);
        }

        /// <summary>
        /// Gets all AHSV components at once as floating point numbers
        /// </summary>
        public void GetAHSVf(out float hue, out float saturation, out float value, out float alpha)
        {
            alpha = _alpha;
            if (_cType != ColorType.HSV)
            {
                ToHSV(out float f, out float s, out float v);
                hue = f;
                saturation = s;
                value = v;
                return;
            }
            hue = _c1;
            saturation = _c2;
            value = _c3;
        }


        /// <summary>
        /// Sets the color as a RGB color, setting all channels at once. Arguments range 0 - 255
        /// </summary>
        public void SetARGB(byte red, byte green, byte blue, byte alpha = 255)
        {
            _alpha = alpha;
            _c1 = red / 255f;
            _c2 = green / 255f;
            _c3 = blue / 255f;
            _cType = ColorType.RGB;
        }

        /// <summary>
        /// Sets the color as an RGB color using floating point numbers, setting all channels
        /// at once. Arguments range 0 - 1
        /// </summary>
        public void SetARGBf(float red, float green, float blue, float alpha = 1.0f)
        {
            _alpha = alpha;
            _c1 = red;
            _c2 = green;
            _c3 = blue;
            _cType = ColorType.HSV;
        }

        /// <summary>
        /// Sets the color as a HSV color, setting all channels
        /// at once. Hue must be > 0, generally [0,360), all other arguments in range 0 - 255
        /// </summary>
        public void SetHSV(int hue, int saturation, int value, int alpha = 255)
        {
            if (hue < 0 || saturation < 0 || saturation > 255 || value < 0 || value > 255 || alpha < 0 || alpha > 255)
                throw new ArgumentOutOfRangeException();

            _alpha = alpha;
            _c1 = hue % 360;
            _c2 = saturation / 255f;
            _c3 = value / 255f;
            _cType = ColorType.HSV;
        }

        /// <summary>
        /// Sets the color as a HSV color using floating point numbers, setting all channels
        /// at once. Hue must be > 0, generally [0,360), all other arguments in range 0 - 1;
        /// </summary>
        public void SetHSVf(float hue, float saturation, float value, float alpha = 1.0f)
        {
            if (hue < 0 || saturation < 0 || saturation > 1 || value < 0 || value > 1 || alpha < 0 || alpha > 1)
                throw new ArgumentOutOfRangeException();

            _alpha = alpha;
            _c1 = hue % 360;
            _c2 = saturation;
            _c3 = value;
            _cType = ColorType.HSV;
        }


        //https://en.wikipedia.org/wiki/HSL_and_HSV
        private void ToHSV(out float hue, out float saturation, out float value)
        {
            if (_cType == ColorType.HSV)
            {
                hue = _c1;
                saturation = _c2;
                value = _c3;
                return;
            }

            var max = MathF.Max(_c1, MathF.Max(_c2, _c3));
            var min = MathF.Min(_c1, MathF.Min(_c2, _c3));
            var range = max - min;

            float h = 0.0f;
            if (range <= EPSILON)
            {
                hue = -1; //acromatic or undefined
                value = max;
                saturation = max == 0 ? 0 : range / max;
                return;
            }
            else if (max == _c1)
            {
                h = ((_c2 - _c3) / range) % 6;
            }
            else if (max == _c2)
            {
                h = ((_c3 - _c1) / range) + 2;
            }
            else if (max == _c3)
            {
                h = ((_c1 - _c2) / range) + 4;
            }

            h *= 60;

            if (h < 0)
                h += 360.0f;

            hue = h;
            value = max;
            saturation = max == 0 ? 0 : range / max;
        }

        private void ToRGB(out float red, out float green, out float blue)
        {
            if (_cType == ColorType.RGB)
            {
                red = _c1;
                green = _c2;
                blue = _c3;
                return;
            }
            if (_c1 == -1) //Achromatic/Undefined
            {
                var ch = _c3 - _c2; //Saturation should be 0 here
                red = green = blue = ch; //Its a grey color
                return;
            }

            var c = _c3 * _c2;
            var h = _c1 / 60;
            var x = c * (1 - MathF.Abs(h % 2 - 1));

            float r = 0;
            float g = 0;
            float b = 0;
            if (h <= 1)
            {
                r = c;
                g = x;
                b = 0;
            }
            else if (h > 1 && h <= 2)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (h > 2 && h <= 3)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (h > 3 && h <= 4)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (h > 4 && h <= 5)
            {
                r = x;
                g = 0;
                b = c;
            }
            else if (h > 5 && h <= 6)
            {
                r = c;
                g = 0;
                b = x;
            }

            var m = _c3 - c;

            red = r + m;
            green = g + m;
            blue = b + m;



            //uses alternate HSV to RGB algorithm
            //https://en.wikipedia.org/wiki/HSL_and_HSV
            //Shouldn't have any issues unless desiring extremely high precision
            //float hue = _c1;
            //float saturation = _c2;
            //float value = _c3;
            //float comp(int n)
            //{
            //    var k = (n + (hue / 60f)) % 6;
            //    var fn = value - (value * saturation) * MathF.Max(0, MathF.Min(k, MathF.Min(4 - k, 1)));
            //    return fn;
            //}

            //red = comp(5);
            //green = comp(3);
            //blue = comp(1);
        }

        /// <summary>
        /// Helper method to convert between color spaces without creating Color2 instances
        /// </summary>
        /// <param name="red">Red [0-255]</param>
        /// <param name="green">Green [0-255]</param>
        /// <param name="blue">Blue [0-255]</param>
        /// <param name="hue">Hue [0-360)</param>
        /// <param name="saturation">Saturation [0-255]</param>
        /// <param name="value">Value [0-255]</param>
        public static void RGBToHSV(byte red, byte green, byte blue, out int hue, out int saturation, out int value)
        {
            var r = red / 255f;
            var g = green / 255f;
            var b = blue / 255f;

            var max = MathF.Max(r, MathF.Max(g, b));
            var min = MathF.Min(r, MathF.Min(g, b));
            var range = max - min;

            float h = 0.0f;
            if (range <= EPSILON)
            {
                h = -1; //acromatic or undefined
            }
            else if (max == r)
            {
                h = ((g - b) / range) % 6;
            }
            else if (max == g)
            {
                h = ((b - r) / range) + 2;
            }
            else if (max == b)
            {
                h = ((r - g) / range) + 4;
            }

            hue = h == -1 ? -1 : (int)MathF.Round(h * 60);
            value = (int)MathF.Round(max * 255);

            saturation = max == 0 ? 0 : (int)MathF.Round(range / max * 255);
        }

        /// <summary>
        /// Helper method to convert between color spaces without creating Color2 instances
        /// </summary>
        /// <param name="red">Red [0-1]</param>
        /// <param name="green">Green [0-1]</param>
        /// <param name="blue">Blue [0-1]</param>
        /// <param name="hue">Hue [0-360)</param>
        /// <param name="saturation">Saturation [0-1]</param>
        /// <param name="value">Value [0-1]</param>
        public static void RGBfToHSVf(float red, float green, float blue, out float hue, out float saturation, out float value)
        {
            var max = MathF.Max(red, MathF.Max(green, blue));
            var min = MathF.Min(red, MathF.Min(green, blue));
            var range = max - min;

            float h = 0.0f;
            if (range <= EPSILON)
            {
                h = -1; //acredomatic ored undefined
            }
            else if (max == red)
            {
                h = ((green - blue) / range) % 6;
            }
            else if (max == green)
            {
                h = ((blue - red) / range) + 2;
            }
            else if (max == blue)
            {
                h = ((red - green) / range) + 4;
            }

            hue = h == -1 ? -1 : h * 60;
            value = max;

            saturation = max == 0 ? 0 : range / max;
        }

        /// <summary>
        /// Helper method to convert between color spaces without creating Color2 instances
        /// </summary>
        /// <param name="hue">Hue [0,360)</param>
        /// <param name="saturation">Saturation [0,255]</param>
        /// <param name="value">Value [0,255]</param>
        /// <param name="red">Red [0,255]</param>
        /// <param name="green">Green [0,255]</param>
        /// <param name="blue">Blue [0,255]</param>
        public static void HSVToRGB(int hue, int saturation, int value, out byte red, out byte green, out byte blue)
        {
            if (hue == -1) //Achromatic/Undefined
            {
                var ch = value - saturation; //Saturation should be 0 here
                red = green = blue = (byte)MathF.Round(ch * 255); //Its a grey color
                return;
            }

            var s = saturation / 255f;
            var v = value / 255f;

            var c = v * s;
            var h = hue / 60;
            var x = c * (1 - MathF.Abs(h % 2 - 1));

            float r = 0;
            float g = 0;
            float b = 0;
            if (h <= 1)
            {
                r = c;
                g = x;
                b = 0;
            }
            else if (h > 1 && h <= 2)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (h > 2 && h <= 3)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (h > 3 && h <= 4)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (h > 4 && h <= 5)
            {
                r = x;
                g = 0;
                b = c;
            }
            else if (h > 5 && h <= 6)
            {
                r = c;
                g = 0;
                b = x;
            }

            var m = v - c;

            red = (byte)MathF.Round((r + m) * 255);
            green = (byte)MathF.Round((g + m) * 255);
            blue = (byte)MathF.Round((b + m) * 255);
        }

        /// <summary>
        /// Helper method to convert between color spaces without creating Color2 instances
        /// </summary>
        /// <param name="hue">Hue [0,360)</param>
        /// <param name="saturation">Saturation [0,1]</param>
        /// <param name="value">Value [0,1]</param>
        /// <param name="red">Red [0,1]</param>
        /// <param name="green">Green [0,1]</param>
        /// <param name="blue">Blue [0,1]</param>
        public static void HSVfToRGBf(float hue, float saturation, float value, out float red, out float green, out float blue)
        {
            if (hue == -1) //Achromatic/Undefined
            {
                var ch = value - saturation; //Saturation should be 0 here
                red = green = blue = ch; //Its a grey color
                return;
            }

            var c = value * saturation;
            var h = hue / 60;
            var x = c * (1 - MathF.Abs(h % 2 - 1));

            float r = 0;
            float g = 0;
            float b = 0;
            if (h <= 1)
            {
                r = c;
                g = x;
                b = 0;
            }
            else if (h > 1 && h <= 2)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (h > 2 && h <= 3)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (h > 3 && h <= 4)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (h > 4 && h <= 5)
            {
                r = x;
                g = 0;
                b = c;
            }
            else if (h > 5 && h <= 6)
            {
                r = c;
                g = 0;
                b = x;
            }

            var m = value - c;

            red = r + m;
            green = g + m;
            blue = b + m;
        }


        public static void HSVfToHSLf(float hue, float saturation, float value, out float satL, out float light)
        {
            light = value * (1 - saturation / 2f);
            satL = (MathF.Abs(1 - light) < EPSILON || light < EPSILON) ? 0 : (value - light) / MathF.Min(light, 1 - light);
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
            return HashCode.Combine(_cType, _alpha, _c1, _c2, _c3);
        }

        public static bool operator ==(Color2 ec1, Color2 ec2)
        {
            return ec1.Equals(ec2);
        }

        public static bool operator !=(Color2 ec1, Color2 ec2)
        {
            return !ec1.Equals(ec2);
        }

        public string ToHexString(bool includeAlpha = true)
        {
            ToRGB(out float r, out float g, out float b);
            if (includeAlpha)
                return $"#{A:x2}{(int)(r * 255):x2}{(int)(g * 255):x2}{(int)(b * 255):x2}";
            else
                return $"#{(int)(r * 255):x2}{(int)(g * 255):x2}{(int)(b * 255):x2}";
        }

        public string ToHTML(bool includeAlpha = true)
        {
            ToRGB(out float r, out float g, out float b);            
            if (includeAlpha)
                return $"rgba( {(int)(r * 255)}, {(int)(g * 255)}, {(int)(b * 255)}, {A} )";
            else
                return $"rgb( {(int)(r * 255)}, {(int)(g * 255)}, {(int)(b * 255)} )";
        }

        public static Color2 Parse(string value)
        {
            if (TryParse(value, out Color2 ec))
                return ec;

            return Empty;
        }

        public static bool TryParse(ReadOnlySpan<char> value, out Color2 ec)
        {
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
                        ec = FromUInt32(result | (v.Length == 3 ? 0xff000000 : 0u));
                        return true;
                    }
                }
                else if (v.Length == 6 || v.Length == 8)
                {
                    if (uint.TryParse(v, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result))
                    {
                        ec = FromUInt32(result | (v.Length == 6 ? 0xff000000 : 0u));
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
                        ec = FromRGB(r, g, b, a);
                        return true;
                    }

                    ec = FromRGB(r, g, b);
                    return true;
                }
            }

            ec = Empty;
            return false;
        }

        public static implicit operator Color(Color2 ec)
        {
            ec.GetARGB(out byte r, out byte g, out byte b, out byte a);
            return Color.FromArgb(a, r, g, b);
        }

        public static implicit operator Color2(Color c)
        {
            return Color2.FromUInt32(c.ToUint32());
        }

        public Color2 WithHuef(float hue)
        {
            hue = MathF.Max(0, hue % 360);
            if (_cType == ColorType.HSV)
            {
                return FromHSV(hue, _c2, _c3, _alpha);
            }
            ToHSV(out float _, out float sat, out float val);
            return FromHSV(hue, sat, val);
        }

        public Color2 WithSatf(float sat)
        {
            sat = MathF.Max(0, MathF.Min(1, sat));

            if (_cType == ColorType.HSV)
            {
                return FromHSV(_c1, sat, _c3, _alpha);
            }
            ToHSV(out float h, out float _, out float v);
            return FromHSV(h, sat, v, _alpha);
        }

        public Color2 WithValf(float val)
        {
            val = MathF.Max(0, MathF.Min(1, val));

            if (_cType == ColorType.HSV)
            {
                return FromHSV(_c1, _c2, val, _alpha);
            }

            ToHSV(out float h, out float s, out float _);
            return FromHSV(h, s, val, _alpha);
        }

        public Color2 WithRedf(float r)
        {
            r = MathF.Max(0, MathF.Min(r, 1));
            if (_cType == ColorType.RGB)
            {
                return FromRGB(r, _c2, _c3, _alpha);
            }
            ToRGB(out float _, out float g, out float b);
            return FromRGB(r, g, b, _alpha);
        }

        public Color2 WithGreenf(float g)
        {
            g = MathF.Max(0, MathF.Min(g, 1));
            if (_cType == ColorType.RGB)
            {
                return FromRGB(_c1, g, _c3, _alpha);
            }
            ToRGB(out float r, out float _, out float b);
            return FromRGB(r, g, b, _alpha);
        }

        public Color2 WithBluef(float b)
        {
            b = MathF.Max(0, MathF.Min(1, b));
            if (_cType == ColorType.RGB)
            {
                return FromRGB(_c1, _c2, b, _alpha);
            }

            ToRGB(out float r, out float g, out float _);
            return FromRGB(r, g, b, _alpha);
        }

        public Color2 WithAlphaf(float a)
        {
            a = MathF.Max(0, MathF.Min(a, 1));
            if (_cType == ColorType.RGB)
            {
                return FromRGB(_c1, _c2, _c3, a);
            }
            else if (_cType == ColorType.HSV)
            {
                return FromHSV(_c1, _c2, _c3, a);
            }
            else
            {
                return Empty;
            }
        }

        private const float EPSILON = 0.001f;
        private ColorType _cType;
        private float _alpha;
        private float _c1;//Red or Hue
        private float _c2;//Green or Saturation
        private float _c3;//Blue or Value, Lightness
        public static readonly Color2 Empty = new Color2();
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
                return Color2.FromUInt32(c.ToUint32());
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is Color2 c)
            {
                c.GetARGB(out byte r, out byte g, out byte b, out byte a);
                return new Color(a, r, g, b);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
