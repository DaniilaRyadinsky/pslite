using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.UI.Xaml.Controls.Primitives;
//using Windows.Graphics.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ImageLinker2.Models
{
    public sealed class ImageLayer : Control, IDisposable
    {
        public int Id { get; set; }
        private byte R = 1;
        private byte G = 1;
        private byte B = 1;
        private double Opasity = 1;
        private string? Mode = "Normal";

        public SoftwareBitmap? Referense;
        private SoftwareBitmap? Icon;

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set { SetValue(TextProperty, value); NotifyPropertyChanged(); }
        }

        public SoftwareBitmapSource? Source
        {
            get => (SoftwareBitmapSource?)GetValue(SourceProperty);
            set
            {
                SetValue(SourceProperty, value);
                NotifyPropertyChanged();
            }
        }

        public event EventHandler<ImageLayer> DeleteRequested;
        public event Action RenderRequested;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(ImageLayer),
        new PropertyMetadata(default(string)));


        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(SoftwareBitmapSource),
            typeof(ImageLayer),
            new PropertyMetadata(null));


        public ImageLayer()
        {
            DefaultStyleKey = typeof(ImageLayer);
            Referense = null;
            Text = "test";
            Icon = null;
        }

        public ImageLayer(SoftwareBitmap referense, string filename, BitmapDecoder decoder, EventHandler<ImageLayer> _DeleteRequested, Action _RenderRequested)
        {
            DefaultStyleKey = typeof(ImageLayer);
            Referense = referense;
            Text = filename;
            DeleteRequested = _DeleteRequested;
            RenderRequested = _RenderRequested;
            MakeIcon(decoder);
        }



        public byte GetR() { return R; }

        public byte GetB() { return B; }

        public byte GetG() { return G; }

        public double GetOpasity() { return Opasity; }

        public string GetMode() { return Mode; }

        public SoftwareBitmap? GetReferense() { return Referense; }


        private async void MakeIcon(BitmapDecoder decoder)
        {
            BitmapTransform transform = new()
            {
                ScaledWidth = 78,
                ScaledHeight = 78
            };

            SoftwareBitmap icon = await decoder.GetSoftwareBitmapAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage
            );
            Icon = SoftwareBitmap.Copy(icon);
            SoftwareBitmapSource iconSource = new();
            await iconSource.SetBitmapAsync(icon);
            Source = iconSource;
        }

        private async void UpdateIcon()
        {
            if (Icon != null)
            {
                var widthBitmap = Icon.PixelWidth;
                var heightBitmap = Icon.PixelHeight;

                WriteableBitmap resultBitmap = new(widthBitmap, heightBitmap);

                using Stream stream = resultBitmap.PixelBuffer.AsStream();
                    byte[] pixels = new byte[widthBitmap * heightBitmap * 4];
                byte[] resultPixels = new byte[widthBitmap * heightBitmap * 4];
                Icon.CopyToBuffer(pixels.AsBuffer());

                for (int i = 0; i < pixels.Length; i += 4)
                {

                    resultPixels[i] = (byte)(pixels[i] * B * Opasity);       // B
                    resultPixels[i + 1] = (byte)(pixels[i + 1] * G * Opasity); // G
                    resultPixels[i + 2] = (byte)(pixels[i + 2] * R * Opasity); // R
                    resultPixels[i + 3] = 255; // Alpha (оставляем непрозрачным)
                }
                stream.Write(resultPixels, 0, resultPixels.Length);

                var newIcon = SoftwareBitmap.CreateCopyFromBuffer(
                    resultBitmap.PixelBuffer,
                    BitmapPixelFormat.Bgra8,
                    resultBitmap.PixelWidth,
                    resultBitmap.PixelHeight,
                    BitmapAlphaMode.Premultiplied
                );
                SoftwareBitmapSource iconSource = new();
                await iconSource.SetBitmapAsync(newIcon);
                Source?.Dispose();
                Source = iconSource;
            }
        }

        static int Clamp(int value)
        {
            // Ограничьте значение от 0 до 255
            return Math.Max(0, Math.Min(255, value));
        }



        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var image = GetTemplateChild("Thumbnail") as Image;
            if (image != null && Source != null)
            {
                image.Source = Source;
            }

            var textBlock = GetTemplateChild("TextBlockElement") as TextBlock;
            if (textBlock != null)
            {
                textBlock.Text = Text;
            }

            var deleteButton = GetTemplateChild("DeleteButton") as Button;
            if (deleteButton != null)
            {
                deleteButton.Click += DeleteButton_Click;
            }

            var modeCombobox = GetTemplateChild("modeCombobox") as ComboBox;
            if (modeCombobox != null)
            {
                modeCombobox.SelectionChanged += ModeComboBox_SelectionChanged;
            }

            var R_Checkbox = GetTemplateChild("R_Checkbox") as CheckBox;
            if (R_Checkbox != null)
            {
                R_Checkbox.Click += R_Checkbox_Click;
            }

            var G_Checkbox = GetTemplateChild("G_Checkbox") as CheckBox;
            if (G_Checkbox != null)
            {
                G_Checkbox.Click += G_Checkbox_Click;
            }

            var B_Checkbox = GetTemplateChild("B_Checkbox") as CheckBox;
            if (B_Checkbox != null)
            {
                B_Checkbox.Click += B_Checkbox_Click;
            }

            var slider = GetTemplateChild("sliderOpasity") as Slider;
            if (slider != null)
            {
                slider.ValueChanged += Slider_ValueChanged;
            }
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                Opasity = slider.Value / 100;
                RenderRequested?.Invoke();
                UpdateIcon();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this, this);
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Mode = e.AddedItems[0].ToString();
            RenderRequested?.Invoke();
            UpdateIcon();
        }

        private void R_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            R = (byte)(R == 0 ? 1 : 0);
            RenderRequested?.Invoke();
            UpdateIcon();
        }

        private void G_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            G = (byte)(G == 0 ? 1 : 0);
            RenderRequested?.Invoke();
            UpdateIcon();
        }

        private void B_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            B = (byte)(B == 0 ? 1 : 0);
            RenderRequested?.Invoke();
            UpdateIcon();
        }


        private bool _disposed; // Чтобы отслеживать, был ли объект уже освобожден

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Text = null;
                Mode = null;
            }

            // Освобождаем SoftwareBitmap
            Referense?.Dispose();
            Icon?.Dispose();
            Source?.Dispose();

            Referense = null;
            Icon = null;
            Source = null;
            RenderRequested = null;
            DeleteRequested = null;

            _disposed = true;
        }

        ~ImageLayer() => Dispose(false);


    }
}