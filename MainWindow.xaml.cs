using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Graphics.Imaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ImageLinker
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<ImageLayer> layers = new();
        public SoftwareBitmap View;


        public MainWindow()
        {
            this.InitializeComponent();
            LayersList.ItemsSource = layers;
        }


        private async void PickAFileButton_Click(object sender, RoutedEventArgs e)
        {
            //disable the button to avoid double-clicking
            var senderButton = sender as Button;
            senderButton.IsEnabled = false;

            // Clear previous returned file name, if it exists, between iterations of this scenario
            PickAFileOutputTextBlock.Text = "";

            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = App.m_window;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                PickAFileOutputTextBlock.Text = "Picked file: " + file.Name;
                await image_Load(file);
            }
            else
            {
                PickAFileOutputTextBlock.Text = "Operation cancelled.";
            }

            //re-enable the button
            senderButton.IsEnabled = true;
        }



        async Task image_Load(StorageFile? file)
        {
            using var stream = await file.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();


            SoftwareBitmap displayableImage = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);


            var imageLayer = new ImageLayer(displayableImage, file.Name, decoder);
            imageLayer.DeleteRequested += ImageLayer_DeleteRequested;
            imageLayer.RenderRequested += renderImg;
            layers.Add(imageLayer);

            renderImg();
        }

        private void ImageLayer_DeleteRequested(object sender, ImageLayer imageLayer)
        {
            layers.Remove(imageLayer);
            if (layers.Count == 0)
            {
                pictureBox1.Source = null;
                View.Dispose();
                View = null;
            }
            else
                renderImg();
        }

        public async void renderImg()
        {
            if (View == null)
                View = layers[0].Referense;
            else
                await rerenderImg(layers[0].Referense, layers[0].GetR(), layers[0].GetG(), layers[0].GetB(), layers[0].GetOpasity());

            for (var i = 1; i < layers.Count; i++)
            {
                await MixImagesAsync(layers[i].Referense, layers[i].GetMode(), layers[i].GetR(), layers[i].GetG(), layers[i].GetB(), layers[i].GetOpasity());
            }
            //pictureBox1.Source = null;


            var sourse = new SoftwareBitmapSource();
            await sourse.SetBitmapAsync(View);
            pictureBox1.Source = sourse;
        }

        public async Task rerenderImg(SoftwareBitmap bitmap, byte R, byte G, byte B, double Opasity)
        {
            var widthBitmap = bitmap.PixelWidth;
            var heightBitmap = bitmap.PixelHeight;

            WriteableBitmap resultBitmap = new WriteableBitmap(widthBitmap, heightBitmap);

            using (Stream stream = resultBitmap.PixelBuffer.AsStream())
            {
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

                View = SoftwareBitmap.CreateCopyFromBuffer(
                    resultBitmap.PixelBuffer,
                    BitmapPixelFormat.Bgra8,
                    resultBitmap.PixelWidth,
                    resultBitmap.PixelHeight,
                    BitmapAlphaMode.Premultiplied
                );
            }
        }


        private async Task MixImagesAsync(SoftwareBitmap bitmap, string Mode, byte R, byte G, byte B, double Opasity)
        {

            var widthView = View.PixelWidth;
            var heightView = View.PixelHeight;
            var widthBitmap = bitmap.PixelWidth;
            var heightBitmap = bitmap.PixelHeight;

            var width = Math.Max(widthView, widthBitmap);
            var height = Math.Max(heightView, heightBitmap);


            WriteableBitmap resultBitmap = new WriteableBitmap(width, height);


            using (Stream stream = resultBitmap.PixelBuffer.AsStream())
            {
                byte[] pixels1 = new byte[width * height * 4];
                byte[] pixels2 = new byte[width * height * 4];
                byte[] resultPixels = new byte[width * height * 4];

                View.CopyToBuffer(pixels1.AsBuffer());
                bitmap.CopyToBuffer(pixels2.AsBuffer());

                //for (int y = 0; y < height; y++)
                Parallel.For(0, height, y => {
                    for (int x = 0; x < width; x++)
                    {
                        int index = (y * width + x) * 4;

                        byte b1 = GetPixelValue(pixels1, widthView, x, y, 0); // Красный компонент
                        byte g1 = GetPixelValue(pixels1, widthView, x, y, 1); // Зеленый компонент
                        byte r1 = GetPixelValue(pixels1, widthView, x, y, 2); // Синий компонент

                        byte b2 = GetPixelValue(pixels2, widthBitmap, x, y, 0);
                        byte g2 = GetPixelValue(pixels2, widthBitmap, x, y, 1);
                        byte r2 = GetPixelValue(pixels2, widthBitmap, x, y, 2);

                        byte rResult;
                        byte gResult;
                        byte bResult;
                        switch (Mode)
                        {
                            case "Sum":
                                rResult = (byte)((r1 + r2 * R * Opasity) % 255);
                                gResult = (byte)((g1 + g2 * G * Opasity) % 255);
                                bResult = (byte)((b1 + b2 * B * Opasity) % 255);
                                break;
                            case "Dif":
                                rResult = (byte)((r1 - r2 * R * Opasity) % 255);
                                gResult = (byte)((g1 - g2 * G * Opasity) % 255);
                                bResult = (byte)((b1 - b2 * B * Opasity) % 255);
                                break;
                            case "Multy":
                                rResult = (byte)((r1 * r2 * R * Opasity) % 255);
                                gResult = (byte)((g1 * g2 * G * Opasity) % 255);
                                bResult = (byte)((b1 * b2 * B * Opasity) % 255);
                                break;
                            default:
                                rResult = (byte)(r2 * R * Opasity);
                                gResult = (byte)(g2 * G * Opasity);
                                bResult = (byte)(b2 * B * Opasity);
                                break;
                        }

                        // Сложение компонентов


                        // Записываем результат
                        resultPixels[index] = bResult;
                        resultPixels[index + 1] = gResult;
                        resultPixels[index + 2] = rResult;
                        resultPixels[index + 3] = 255;
                    }
                });
                {
                }

                await stream.WriteAsync(resultPixels, 0, resultPixels.Length);

                View = SoftwareBitmap.CreateCopyFromBuffer(
                    resultBitmap.PixelBuffer,
                    BitmapPixelFormat.Bgra8,
                    resultBitmap.PixelWidth,
                    resultBitmap.PixelHeight,
                    BitmapAlphaMode.Premultiplied
                );
            }
        }


        private byte GetPixelValue(byte[] pixels, int width, int x, int y, int channel)
        {
            if (x < 0 || y < 0 || x >= width || y >= (pixels.Length / (width * 4)))
            {
                return 0;
            }

            int index = (y * width + x) * 4 + channel;
            return pixels[index];
        }

    }
}