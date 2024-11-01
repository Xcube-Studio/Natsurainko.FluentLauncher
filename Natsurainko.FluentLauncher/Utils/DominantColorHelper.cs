using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Utils;

internal static class DominantColorHelper
{
    public static async Task<Windows.UI.Color> GetColorFromImageAsync(string imageFile)
    {
        var colorDictionary = new Dictionary<Color, int>();
        using var bitmap = new Bitmap(imageFile);

        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Color pixelColor = bitmap.GetPixel(x, y);
                if (!colorDictionary.ContainsKey(pixelColor))
                    colorDictionary[pixelColor] = 1;
                else colorDictionary[pixelColor] += 1;
            }
        }

        var resultColor = colorDictionary
            .OrderByDescending(kvp => kvp.Value)
            .First(kvp =>
            {
                double brightness = 0.299 * kvp.Key.R + 0.587 * kvp.Key.G + 0.114 * kvp.Key.B;
                return brightness > 128 && brightness < 240;
            }).Key;

        return await Task.FromResult(new Windows.UI.Color
        {
            A = 255,
            R = resultColor.R,
            G = resultColor.G,
            B = resultColor.B,
        });
    }
}
