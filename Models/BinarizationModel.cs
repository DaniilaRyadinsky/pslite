using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ImageLinker2.Models
{
    public static class BinarizationModel
    {

        public static async Task<WriteableBitmap?> MakeGradGray(WriteableBitmap? bitmap)
        {
            if (bitmap != null)
            {
                int widthBitmap = bitmap.PixelWidth;
                int heightBitmap = bitmap.PixelHeight;
                WriteableBitmap resultBitmap = new(widthBitmap, heightBitmap);

                using Stream resultStream = resultBitmap.PixelBuffer.AsStream();
                byte[] pixels = new byte[widthBitmap * heightBitmap * 4];
                byte[] resultPixels = new byte[widthBitmap * heightBitmap * 4];

                using (var stream = bitmap.PixelBuffer.AsStream())
                {
                    await stream.ReadAsync(pixels, 0, pixels.Length);
                }

                Parallel.For(0, heightBitmap, y =>
                {
                    for (int x = 0; x < widthBitmap; x++)
                    {
                        int index = (y * widthBitmap + x) * 4;

                        byte b = pixels[index];
                        byte g = pixels[index + 1];
                        byte r = pixels[index + 2];
                        byte gray = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);

                        resultPixels[index] = gray;
                        resultPixels[index + 1] = gray;
                        resultPixels[index + 2] = gray;
                        resultPixels[index + 3] = 255;
                    }
                });

                await resultStream.WriteAsync(resultPixels, 0, resultPixels.Length);
                resultBitmap.Invalidate();

                return resultBitmap;
            }
            return null;
        }

        internal static async Task<WriteableBitmap?> BinarizationGavr(WriteableBitmap? bitmap)
        {
            if (bitmap != null)
            {
                int widthBitmap = bitmap.PixelWidth;
                int heightBitmap = bitmap.PixelHeight;

                WriteableBitmap? resultBitmap = new(widthBitmap, heightBitmap);

                using Stream resultStream = resultBitmap.PixelBuffer.AsStream();
                byte[] pixels = new byte[widthBitmap * heightBitmap * 4];
                byte[] resultPixels = new byte[widthBitmap * heightBitmap * 4];

                using (var stream = bitmap.PixelBuffer.AsStream())
                {
                    await stream.ReadAsync(pixels, 0, pixels.Length);
                }

                int sum = 0;
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    sum += pixels[i];
                }

                byte threshold = (byte)(sum / (widthBitmap * heightBitmap));

                Parallel.For(0, heightBitmap, y =>
                {
                    for (int x = 0; x < widthBitmap; x++)
                    {
                        int index = (y * widthBitmap + x) * 4;

                        byte gray = pixels[index];

                        byte bin = gray <= threshold ? (byte)0 : (byte)255;
                        resultPixels[index] = resultPixels[index + 1] = resultPixels[index + 2] = bin;
                        resultPixels[index + 3] = 255;
                    }
                });

                await resultStream.WriteAsync(resultPixels, 0, resultPixels.Length);
                resultBitmap.Invalidate();

                return resultBitmap;
            }
            return null;
        }

        internal static async Task<WriteableBitmap?> BinarizationOtsu(WriteableBitmap? bitmap)
        {
            if (bitmap != null)
            {
                int widthBitmap = bitmap.PixelWidth;
                int heightBitmap = bitmap.PixelHeight;
                int sizeImg = widthBitmap * heightBitmap;

                WriteableBitmap? resultBitmap = new(widthBitmap, heightBitmap);

                using Stream resultStream = resultBitmap.PixelBuffer.AsStream();
                byte[] pixels = new byte[widthBitmap * heightBitmap * 4];
                byte[] resultPixels = new byte[widthBitmap * heightBitmap * 4];

                using (var stream = bitmap.PixelBuffer.AsStream())
                {
                    await stream.ReadAsync(pixels, 0, pixels.Length);
                }
                //calculation histogram
                int[] histogram = new int[256];
                Parallel.For(0, heightBitmap, y =>
                {
                    for (int x = 0; x < widthBitmap; x++)
                    {
                        int index = (y * widthBitmap + x) * 4;

                        byte gray = pixels[index];
                        histogram[gray]++;
                    }
                });

                var uT = 0;
                for (int i = 0; i < 256; i++)
                {
                    if (histogram[i] == histogram.Max())
                        break;

                    uT += i * histogram[i];
                }

                float sum = 0;
                for (int i = 0; i < 256; i++)
                    sum += i * histogram[i];

                float sumB = 0, wB = 0, wF = 0;
                float maxVariance = 0;
                int threshold = 0;

                for (int t = 0; t < 256; t++)
                {
                    wB += histogram[t];
                    if (wB == 0) continue;

                    wF = sizeImg - wB;
                    if (wF == 0) break;

                    sumB += t * histogram[t];
                    float mB = sumB / wB;
                    float mF = (sum - sumB) / wF;

                    float variance = wB * wF * (mB - mF) * (mB - mF);
                    if (variance > maxVariance)
                    {
                        maxVariance = variance;
                        threshold = t;
                    }

                    Parallel.For(0, heightBitmap, y =>
                    {
                        for (int x = 0; x < widthBitmap; x++)
                        {
                            int index = (y * widthBitmap + x) * 4;

                            byte gray = pixels[index];

                            byte bin = gray <= threshold ? (byte)0 : (byte)255;
                            resultPixels[index] = resultPixels[index + 1] = resultPixels[index + 2] = bin;
                            resultPixels[index + 3] = 255;
                        }
                    });
                }

                await resultStream.WriteAsync(resultPixels, 0, resultPixels.Length);
                resultBitmap.Invalidate();

                return resultBitmap;
            }
            return null;

        }


        internal static async Task<WriteableBitmap?> BinarizationNibl(WriteableBitmap? bitmap, int windowSize, double k)
        {
            if (bitmap == null)
                return null;
            if (windowSize % 2 == 0)
            {
                return null;
            }

            int widthBitmap = bitmap.PixelWidth;
            int heightBitmap = bitmap.PixelHeight;
            int sizeImg = widthBitmap * heightBitmap;

            WriteableBitmap? resultBitmap = new(widthBitmap, heightBitmap);

            using Stream resultStream = resultBitmap.PixelBuffer.AsStream();
            byte[] pixels = new byte[widthBitmap * heightBitmap * 4];
            byte[] resultPixels = new byte[widthBitmap * heightBitmap * 4];

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                await stream.ReadAsync(pixels, 0, pixels.Length);
            }

            Parallel.For(0, heightBitmap, y =>
                {
                    for (int x = 0; x < widthBitmap; x++)
                    {
                        int index;
                        byte gray;

                        //calculate M(x) and M(x^2)
                        int winSum = 0;
                        int winSumSquare = 0;
                        int count = 0;

                        for (int winY = -windowSize / 2; winY < windowSize / 2; winY++)
                        {
                            for (int winX = -windowSize / 2; winX < windowSize / 2; winX++)
                            {
                                int nX = x + winX, nY = y + winY;
                                if (nX >= 0 && nY >= 0 && nX < widthBitmap && nY < heightBitmap)
                                {
                                    index = (nY * widthBitmap + nX) * 4;

                                    gray = pixels[index];

                                    winSum += gray;
                                    winSumSquare += gray * gray;
                                    count++;
                                }
                            }
                        }
                        //calculate threshold
                        double MX = winSum / count;
                        double DX = winSumSquare / count - Math.Pow(MX, 2);

                        double delta = Math.Sqrt(DX);
                        double threshold = MX + k * delta;

                        //layout
                        index = (y * widthBitmap + x) * 4;

                        gray = pixels[index];

                        byte bin = gray < threshold ? (byte)0 : (byte)255;

                        resultPixels[index] = resultPixels[index + 1] = resultPixels[index + 2] = bin;
                        resultPixels[index + 3] = 255;
                    }
                });

            await resultStream.WriteAsync(resultPixels, 0, resultPixels.Length);
            resultBitmap.Invalidate();

            return resultBitmap;
        }

        internal static async Task<WriteableBitmap?> BinarizationRot(WriteableBitmap? bitmap)
        {
            throw new NotImplementedException();
        }

        internal static async Task<WriteableBitmap?> BinarizationSauv(WriteableBitmap? bitmap, int windowSize, double k)
        {
            if (bitmap == null)
                return null;
            if (windowSize % 2 == 0)
            {
                return null;
            }

            int widthBitmap = bitmap.PixelWidth;
            int heightBitmap = bitmap.PixelHeight;
            int sizeImg = widthBitmap * heightBitmap;

            WriteableBitmap? resultBitmap = new(widthBitmap, heightBitmap);

            using Stream resultStream = resultBitmap.PixelBuffer.AsStream();
            byte[] pixels = new byte[widthBitmap * heightBitmap * 4];
            byte[] resultPixels = new byte[widthBitmap * heightBitmap * 4];

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                await stream.ReadAsync(pixels, 0, pixels.Length);
            }

            Parallel.For(0, heightBitmap, y =>
            {
                for (int x = 0; x < widthBitmap; x++)
                {
                    int index;
                    byte gray;

                    //calculate M(x) and M(x^2)
                    int winSum = 0;
                    int winSumSquare = 0;
                    int count = 0;

                    for (int winY = -windowSize / 2; winY < windowSize / 2; winY++)
                    {
                        for (int winX = -windowSize / 2; winX < windowSize / 2; winX++)
                        {
                            int nX = x + winX, nY = y + winY;
                            if (nX >= 0 && nY >= 0 && nX < widthBitmap && nY < heightBitmap)
                            {
                                index = (nY * widthBitmap + nX) * 4;

                                gray = pixels[index];

                                winSum += gray;
                                winSumSquare += gray * gray;
                                count++;
                            }
                        }
                    }
                    //calculate threshold
                    double MX = winSum / count;
                    double DX = winSumSquare / count - Math.Pow(MX, 2);

                    double delta = Math.Sqrt(DX);
                    double threshold = MX * (1 + k * (delta / 128 - 1));

                    //layout
                    index = (y * widthBitmap + x) * 4;

                    gray = pixels[index];

                    byte bin = gray < threshold ? (byte)0 : (byte)255;

                    resultPixels[index] = resultPixels[index + 1] = resultPixels[index + 2] = bin;
                    resultPixels[index + 3] = 255;
                }
            });

            await resultStream.WriteAsync(resultPixels, 0, resultPixels.Length);
            resultBitmap.Invalidate();

            return resultBitmap;
        }

        internal static async Task<WriteableBitmap?> BinarizationWulf(WriteableBitmap? bitmap, int windowSize, double a)
        {
            if (bitmap == null) return null;
            if (windowSize % 2 == 0)
            {
                return null;
            }

            int widthBitmap = bitmap.PixelWidth;
            int heightBitmap = bitmap.PixelHeight;
            int sizeImg = widthBitmap * heightBitmap;

            WriteableBitmap? resultBitmap = new(widthBitmap, heightBitmap);

            using Stream resultStream = resultBitmap.PixelBuffer.AsStream();
            byte[] pixels = new byte[widthBitmap * heightBitmap * 4];
            byte[] resultPixels = new byte[widthBitmap * heightBitmap * 4];

            using (var stream = bitmap.PixelBuffer.AsStream())
            {
                await stream.ReadAsync(pixels, 0, pixels.Length);
            }

            byte minI = 255;
            double maxDelta = -double.MinValue;

            //searh maxDelta and minI
            Parallel.For(0, heightBitmap, y =>
                {
                    for (int x = 0; x < widthBitmap; x++)
                    {
                        int index;
                        byte gray;

                        //calculate M(x) and M(x^2)
                        int winSum = 0;
                        int winSumSquare = 0;
                        int count = 0;

                        for (int winY = -windowSize / 2; winY < windowSize / 2; winY++)
                        {
                            for (int winX = -windowSize / 2; winX < windowSize / 2; winX++)
                            {
                                int nX = x + winX, nY = y + winY;
                                if (nX >= 0 && nY >= 0 && nX < widthBitmap && nY < heightBitmap)
                                {
                                    index = (nY * widthBitmap + nX) * 4;

                                    gray = pixels[index];

                                    winSum += gray;
                                    winSumSquare += gray * gray;
                                    count++;
                                }
                            }
                        }
                        //calculate threshold
                        double MX = winSum / count;
                        double DX = winSumSquare / count - Math.Pow(MX, 2);

                        double delta = Math.Sqrt(DX);

                        index = (y * widthBitmap + x) * 4;
                        gray = pixels[index];
                        //lock (this)
                        //{
                        minI = Math.Min(minI, gray);
                        maxDelta = Math.Max(delta, maxDelta);
                        //}
                    }
                });

            //calc + layout
            Parallel.For(0, heightBitmap, y =>
            {
                for (int x = 0; x < widthBitmap; x++)
                {
                    int index;
                    byte b, g, r, gray;

                    //calculate M(x) and M(x^2)
                    int winSum = 0;
                    int winSumSquare = 0;
                    int count = 0;

                    for (int winY = -windowSize / 2; winY < windowSize / 2; winY++)
                    {
                        for (int winX = -windowSize / 2; winX < windowSize / 2; winX++)
                        {
                            int nX = x + winX, nY = y + winY;
                            if (nX >= 0 && nY >= 0 && nX < widthBitmap && nY < heightBitmap)
                            {
                                index = (nY * widthBitmap + nX) * 4;

                                gray = pixels[index];

                                winSum += gray;
                                winSumSquare += gray * gray;
                                count++;
                            }
                        }
                    }
                    //calculate threshold
                    double MX = winSum / count;
                    double DX = winSumSquare / count - Math.Pow(MX, 2);

                    double delta = Math.Sqrt(DX);

                    double threshold = (1 - a) * MX + a * minI + a * delta / maxDelta * (MX - minI);

                    //layout
                    index = (y * widthBitmap + x) * 4;

                    gray = pixels[index];

                    byte bin = gray < threshold ? (byte)0 : (byte)255;

                    resultPixels[index] = resultPixels[index + 1] = resultPixels[index + 2] = bin;
                    resultPixels[index + 3] = 255;
                }
            });

            await resultStream.WriteAsync(resultPixels, 0, resultPixels.Length);
            resultBitmap.Invalidate();

            return resultBitmap;
        }
    }
}
