using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;

namespace ImageLinker2.Models
{
    public static class Curve
    {

        public async static Task<WriteableBitmap?> MakeHistogram(WriteableBitmap? View)
        {
            if (View != null)
            {
                int HistogramWidth = 256;
                int HistogramHeight = 256;
                int[] brightness = await CalculateBrightness(View, HistogramHeight);

                WriteableBitmap resultBitmap = new(HistogramWidth, HistogramHeight);

                using Stream stream = resultBitmap.PixelBuffer.AsStream();

                byte[] pixels = new byte[HistogramWidth * HistogramHeight * 4];

                for (int i = 0; i < pixels.Length; i += 4)
                {
                    var x = (i / 4) % HistogramWidth;
                    var y = (i / 4) / HistogramWidth;
                    if ((HistogramHeight - brightness[x] - 1) <= y)
                    {
                        pixels[i] = 0;
                        pixels[i + 1] = 0;
                        pixels[i + 2] = 0;
                        pixels[i + 3] = 255;
                    }
                    else
                    {
                        pixels[i] = 255;
                        pixels[i + 1] = 255;
                        pixels[i + 2] = 255;
                        pixels[i + 3] = 255;
                    }

                }

                await stream.WriteAsync(pixels, 0, pixels.Length);
                resultBitmap.Invalidate();

                return resultBitmap;
            }
            return null;
        }

        public static async Task<int[]> CalculateBrightness(WriteableBitmap View, int HistogramHeight)
        {

            var result = new int[256];
            var widthBitmap = View.PixelWidth;
            var heightBitmap = View.PixelHeight;

            byte[] pixels = new byte[widthBitmap * heightBitmap * 4];
            using (var stream = View.PixelBuffer.AsStream())
            {
                await stream.ReadAsync(pixels, 0, pixels.Length);
            }


            for (int i = 0; i < pixels.Length; i += 4)
            {
                var brightness = (pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3;
                result[brightness]++;
            }

            Normalize(result, HistogramHeight);

            return result;

        }

        private static void Normalize(int[] brightness, int HistogramHeight)
        {
            double brightnessKoff = (HistogramHeight * 1.0) / brightness.Max();
            for (int i = 0; i < brightness.Length; i++)
            {
                brightness[i] = (int)(brightness[i] * brightnessKoff);
            }
        }

        public static Dictionary<int, int> GenerateTransformMap(List<Point> points)
        {
            Dictionary<int, int> map = new(256);
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];

                for (int x = (int)p1.X; x <= (int)p2.X; x++)
                {
                    map[x] = (int)((255-p1.Y) + ((255 -p2.Y) - (255-p1.Y)) / (p2.X - p1.X) * (x - p1.X)); // f(x0) + (f(x1) - f(x0))/(x1 - x0) * (x - x0)
                }
            }
            return map;
        }
    }
}
