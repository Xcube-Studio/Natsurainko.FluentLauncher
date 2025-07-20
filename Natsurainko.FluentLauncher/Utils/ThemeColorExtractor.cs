using System;
using System.Linq;
using System.Numerics;
using SharpDX.WIC;

namespace Natsurainko.FluentLauncher.Utils;

internal class ThemeColorExtractor
{
    public static Windows.UI.Color[] GetThemeColors(string imageFile, int clusterCount = 9)
    {
        using var imagingFactory = new ImagingFactory();
        using var wicBitmap = LoadAndResizeWicBitmap(imagingFactory, imageFile);

        Vector3[] pixelVectors = ConvertPixelsToVectors(wicBitmap);
        Vector3[] centroids = KMeans(pixelVectors, clusterCount);

        Windows.UI.Color[] colors = [
            .. centroids.Select(v => Windows.UI.Color.FromArgb(255, (byte)v.X, (byte)v.Y, (byte)v.Z))
        ];

        pixelVectors = null!;
        centroids = null!;

        GC.Collect();

        return colors;
    }

    private static FormatConverter LoadAndResizeWicBitmap(ImagingFactory factory, string file)
    {
        using var decoder = new BitmapDecoder(factory, file, DecodeOptions.CacheOnDemand);
        using var frame = decoder.GetFrame(0);

        int width = Math.Min(512, frame.Size.Width);
        int height = Math.Min(512, frame.Size.Height);

        using var scaler = new BitmapScaler(factory);
        scaler.Initialize(frame, width, height, BitmapInterpolationMode.HighQualityCubic);

        var converter = new FormatConverter(factory);
        converter.Initialize(scaler, PixelFormat.Format32bppBGRA);

        return converter;
    }

    private static Vector3[] ConvertPixelsToVectors(BitmapSource wicBitmap)
    {
        int width = wicBitmap.Size.Width;
        int height = wicBitmap.Size.Height;
        int stride = width * 4;
        byte[] pixels = new byte[height * stride];
        wicBitmap.CopyPixels(pixels, stride);

        Vector3[] pixelVectors = new Vector3[width * height];
        int index = 0;

        for (int y = 0; y < height; y++)
        {
            int line = y * stride;
            for (int x = 0; x < width; x++)
            {
                int idx = line + x * 4;
                byte b = pixels[idx];
                byte g = pixels[idx + 1];
                byte r = pixels[idx + 2];
                byte a = pixels[idx + 3];

                if (a == 0) continue;

                pixelVectors[index++] = new Vector3(r, g, b);
            }
        }

        if (index < pixelVectors.Length)
            Array.Resize(ref pixelVectors, index);

        return pixelVectors;
    }

    private static Vector3[] KMeans(ReadOnlySpan<Vector3> pixelVectors, int k, int maxIter = 20)
    {
        Span<Vector3> centroids = RandomTake(pixelVectors, k);
        int[] labels = new int[pixelVectors.Length];

        for (int iter = 0; iter < maxIter; iter++)
        {
            for (int i = 0; i < pixelVectors.Length; i++)
            {
                int label = -1;
                double minDist = double.MaxValue;

                for (int j = 0; j < k; j++)
                {
                    double dist = Vector3.Distance(pixelVectors[i], centroids[j]);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        label = j;
                    }
                }

                labels[i] = label;
            }

            var sum = new Vector3[k];
            var count = new int[k];

            for (int i = 0; i < pixelVectors.Length; i++)
            {
                int label = labels[i];

                sum[label] += pixelVectors[i];
                count[label]++;
            }

            for (int i = 0; i < k; i++)
                if (count[i] > 0)
                    centroids[i] = sum[i] / count[i];

            GC.Collect();
        }

        return centroids.ToArray();
    }

    private static Span<Vector3> RandomTake(ReadOnlySpan<Vector3> data, int count)
    {
        Random rand = new();
        Span<Vector3> result = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            Vector3 randomData = data[rand.Next(data.Length)];

            while (result.Contains(randomData))
                randomData = data[rand.Next(data.Length)];

            result[i] = randomData;
        }

        return result;
    }

    // Note: I had figured out that the issue is related to the image format (webp)

    // System.Drawing.Common is a wrapper for GDI+, and GDI+ itself does not support the webp format
    // (although you can install the Webp image extension for Windows, but this will not help)

    // There are issues on GitHub about this:

    // https://github.com/dotnet/winforms/issues/11704
    // https://github.com/dotnet/runtime/issues/76237#issuecomment-1469009299

    // Finally, I decided to use SharpDX.WIC, which is a wrapper for Windows Imaging Component (WIC).

    // Note: The original code using System.Drawing.Bitmap is commented out.

    // What's wrong with the System.Drawing.Bitmap ctor?
    // Why new Bitmap(imageFile) throws ArgumentException for some images?
    // PInvoke.GdiplusCreateBitmapFromFile throw ArgumentException : The parameter is not valid.
    // It works in most cases, but some images cause this exception.

    //public static Windows.UI.Color[] GetThemeColors(string imageFile, int clusterCount = 9)
    //{
    //    using var bitmap = new Bitmap(imageFile);
    //    using var resizedBitmap = ResizeBitmap(bitmap);

    //    ReadOnlySpan<Vector3> pixelVectors = ConvertPixelsToVectors(bitmap);
    //    Vector3[] centroids = KMeans(pixelVectors, clusterCount);

    //    Windows.UI.Color[] colors = [.. 
    //        centroids.Select(v => Windows.UI.Color.FromArgb(255, (byte)v.X, (byte)v.Y, (byte)v.Z))];

    //    pixelVectors = null;
    //    centroids = null!;

    //    GC.Collect();

    //    return colors;
    //}

    //private static Bitmap ResizeBitmap(Bitmap source)
    //{
    //    (int width, int height) = (Math.Min(512, source.Width), Math.Min(512, source.Height));
    //    Bitmap result = new(width, height);

    //    using (Graphics g = Graphics.FromImage(result))
    //    {
    //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
    //        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
    //        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
    //        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
    //        g.DrawImage(source, 0, 0, width, height);
    //    }

    //    return result;
    //}

    //private static ReadOnlySpan<Vector3> ConvertPixelsToVectors(Bitmap bitmap)
    //{
    //    Span<Vector3> pixelVectors = new Vector3[bitmap.Width * bitmap.Height];
    //    int index = 0;

    //    for (int y = 0; y < bitmap.Height; y++)
    //    {
    //        for (int x = 0; x < bitmap.Width; x++)
    //        {
    //            Color color = bitmap.GetPixel(x, y);

    //            if (color.A == 0) 
    //                continue;

    //            pixelVectors[index] = new Vector3(color.R, color.G, color.B);
    //            index++;
    //        }
    //    }

    //    return pixelVectors;
    //}
}