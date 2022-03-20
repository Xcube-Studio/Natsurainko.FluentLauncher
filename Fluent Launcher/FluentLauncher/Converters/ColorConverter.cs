using System;
using Windows.UI;

namespace FluentLauncher.Converters
{
    public class ColorConverter
    {
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
