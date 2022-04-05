using System;
using Windows.UI;

namespace FluentLauncher.Converters
{
    public class ColorConverter
    {
        /// <summary>
        /// Creates color with corrected brightness.
        /// </summary>
        /// <param name="color">Color to correct.</param>
        /// <param name="correctionFactor">The brightness correction factor. Must be between -1 and 1. 
        /// Negative values produce darker colors.</param>
        /// <returns>
        /// Corrected <see cref="Color"/> structure.
        /// </returns>
        public static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (byte)(int)red, (byte)(int)green, (byte)(int)blue);
        }

        public static Color FromString(string hex)
        {
            hex = hex.Replace("#", string.Empty);

            //#FFDFD991
            //#DFD991
            //#FD92
            //#DAC

            bool existAlpha = hex.Length == 8 || hex.Length == 4;
            bool isDoubleHex = hex.Length == 8 || hex.Length == 6;

            if (!existAlpha && hex.Length != 6 && hex.Length != 3)
            {
                throw new ArgumentException("输入的hex不是有效颜色");
            }

            int n = 0;
            byte a;
            int hexCount = isDoubleHex ? 2 : 1;
            if (existAlpha)
            {
                n = hexCount;
                a = (byte)ConvertHexToByte(hex, 0, hexCount);
                if (!isDoubleHex)
                {
                    a = (byte)(a * 16 + a);
                }
            }
            else
            {
                a = 0xFF;
            }

            var r = (byte)ConvertHexToByte(hex, n, hexCount);
            var g = (byte)ConvertHexToByte(hex, n + hexCount, hexCount);
            var b = (byte)ConvertHexToByte(hex, n + 2 * hexCount, hexCount);
            if (!isDoubleHex)
            {
                //#FD92 = #FFDD9922

                r = (byte)(r * 16 + r);
                g = (byte)(g * 16 + g);
                b = (byte)(b * 16 + b);
            }

            return Color.FromArgb(a, r, g, b);
        }

        private static uint ConvertHexToByte(string hex, int n, int count = 2)
        {
            return Convert.ToUInt32(hex.Substring(n, count), 16);
        }
    }
}
