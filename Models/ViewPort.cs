using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using ImageLinker2.ViewModel;
using Windows.Foundation;

namespace ImageLinker2.Models
{
    public static class ViewPort
    {

        public static async Task<WriteableBitmap?> Copy(SoftwareBitmap? bitmap)
        {
            WriteableBitmap resultBitmap = new(bitmap.PixelWidth, bitmap.PixelHeight);

            using Stream stream = resultBitmap.PixelBuffer.AsStream();

            byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            bitmap.CopyToBuffer(pixels.AsBuffer());

            await stream.WriteAsync(pixels, 0, pixels.Length);
            resultBitmap.Invalidate();

            return resultBitmap;
        }

        public static async Task<WriteableBitmap?> UpdateViewPort(SoftwareBitmap? bitmap, byte R, byte G, byte B, double Opasity)
        {
            if (bitmap != null)
            {
                var widthBitmap = bitmap.PixelWidth;
                var heightBitmap = bitmap.PixelHeight;

                WriteableBitmap resultBitmap = new(widthBitmap, heightBitmap);

                using Stream stream = resultBitmap.PixelBuffer.AsStream();

                byte[] pixels = new byte[widthBitmap * heightBitmap * 4];
                byte[] resultPixels = new byte[widthBitmap * heightBitmap * 4];
                bitmap.CopyToBuffer(pixels.AsBuffer());

                for (int i = 0; i < pixels.Length; i += 4)
                {

                    resultPixels[i] = (byte)(pixels[i] * B * Opasity);       // B
                    resultPixels[i + 1] = (byte)(pixels[i + 1] * G * Opasity); // G
                    resultPixels[i + 2] = (byte)(pixels[i + 2] * R * Opasity); // R
                    resultPixels[i + 3] = 255; // Alpha (оставляем непрозрачным)
                }
                await stream.WriteAsync(resultPixels, 0, resultPixels.Length);
                resultBitmap.Invalidate();

                return resultBitmap;
            }
            return null;
        }

        public static async Task<WriteableBitmap?> BuildViewPort(WriteableBitmap? bitmap1, SoftwareBitmap? bitmap2, string Mode, byte R, byte G, byte B, double Opasity)
        {
            if (bitmap1 != null && bitmap2 != null)
            {
                var widthView = bitmap1.PixelWidth;
                var heightView = bitmap1.PixelHeight;
                var widthBitmap = bitmap2.PixelWidth;
                var heightBitmap = bitmap2.PixelHeight;

                var width = Math.Max(widthView, widthBitmap);
                var height = Math.Max(heightView, heightBitmap);

                WriteableBitmap resultBitmap = new(width, height);

                using Stream resultStream = resultBitmap.PixelBuffer.AsStream();

                byte[] pixels1 = new byte[width * height * 4];
                byte[] pixels2 = new byte[width * height * 4];
                byte[] resultPixels = new byte[width * height * 4];

                using var stream = bitmap1.PixelBuffer.AsStream();
                await stream.ReadAsync(pixels1, 0, pixels1.Length);

                bitmap2.CopyToBuffer(pixels2.AsBuffer());

                //for (int y = 0; y < height; y++)
                Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = (y * width + x) * 4;

                        byte b1 = GetPixelValue(pixels1, widthView, x, y, 0);
                        byte g1 = GetPixelValue(pixels1, widthView, x, y, 1);
                        byte r1 = GetPixelValue(pixels1, widthView, x, y, 2);

                        byte b2 = GetPixelValue(pixels2, widthBitmap, x, y, 0);
                        byte g2 = GetPixelValue(pixels2, widthBitmap, x, y, 1);
                        byte r2 = GetPixelValue(pixels2, widthBitmap, x, y, 2);

                        byte rResult;
                        byte gResult;
                        byte bResult;
                        switch (Mode)
                        {
                            case "Sum":
                                rResult = (byte)(r1 + r2 * R * Opasity);
                                gResult = (byte)(g1 + g2 * G * Opasity);
                                bResult = (byte)(b1 + b2 * B * Opasity);
                                break;
                            case "Dif":
                                rResult = (byte)(r1 - r2 * R * Opasity);
                                gResult = (byte)(g1 - g2 * G * Opasity);
                                bResult = (byte)(b1 - b2 * B * Opasity);
                                break;
                            case "Multy":
                                rResult = (byte)(r1 * r2 * R * Opasity);
                                gResult = (byte)(g1 * g2 * G * Opasity);
                                bResult = (byte)(b1 * b2 * B * Opasity);
                                break;
                            default:
                                rResult = (byte)Clamp((int)(r1 + r2 * R * Opasity));
                                gResult = (byte)Clamp((int)(g1 + g2 * G * Opasity));
                                bResult = (byte)Clamp((int)(b1 + b2 * B * Opasity));
                                break;
                        }
                        resultPixels[index] = bResult;
                        resultPixels[index + 1] = gResult;
                        resultPixels[index + 2] = rResult;
                        resultPixels[index + 3] = 255;
                    }
                });

                await resultStream.WriteAsync(resultPixels, 0, resultPixels.Length);

                resultBitmap.Invalidate();
                return resultBitmap;
            }
            return null;

        }

        public static async Task<WriteableBitmap?> UpdateViewPort(WriteableBitmap? bitmap, Dictionary<int, int> map)
        {
            if (bitmap != null)
            {
                var widthBitmap = bitmap.PixelWidth;
                var heightBitmap = bitmap.PixelHeight;

                WriteableBitmap resultBitmap = new(widthBitmap, heightBitmap);

                using Stream resultStream = resultBitmap.PixelBuffer.AsStream();

                byte[] pixels = new byte[widthBitmap * heightBitmap * 4];
                byte[] resultPixels = new byte[widthBitmap * heightBitmap * 4];
                using (var stream = bitmap.PixelBuffer.AsStream())
                {
                    await stream.ReadAsync(pixels, 0, pixels.Length);
                }

                for (int i = 0; i < pixels.Length; i += 4)
                {

                    resultPixels[i] = (byte)(map[pixels[i]]);
                    resultPixels[i + 1] = (byte)(map[pixels[i + 1]]);
                    resultPixels[i + 2] = (byte)(map[pixels[i + 2]]); // R
                    resultPixels[i + 3] = 255; // Alpha (оставляем непрозрачным)
                }
                await resultStream.WriteAsync(resultPixels, 0, resultPixels.Length);

                resultBitmap.Invalidate();

                return resultBitmap;
            }
            return null;
        }


        static int Clamp(int value)
        {
            // Ограничьте значение от 0 до 255
            return Math.Max(0, Math.Min(255, value));
        }

        private static byte GetPixelValue(byte[] pixels, int width, int x, int y, int channel)
        {
            if (x < 0 || y < 0 || x >= width || y >= pixels.Length / (width * 4))
            {
                return 0;
            }

            int index = (y * width + x) * 4 + channel;
            return pixels[index];
        }
    }
}
