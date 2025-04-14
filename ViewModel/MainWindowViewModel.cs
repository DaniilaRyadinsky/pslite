using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;
using ImageLinker2.Models;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ImageLinker2.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public LayersViewModel LayersVM;
        public ViewPortViewModel ViewPortVM;
        public CurviesViewModel CurviesVM;
        public BinarizationViewModel BinarizationVM;

        private string? _text;
        public string? Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }


        public MainWindowViewModel()
        {
            LayersVM = new();
            ViewPortVM = new ViewPortViewModel();
            CurviesVM = new CurviesViewModel(ViewPortVM);
            BinarizationVM = new BinarizationViewModel(ViewPortVM);
        }

        public async void PickAFileButton_Click(object sender, RoutedEventArgs e)
        {
            //disable the button to avoid double-clicking
            var senderButton = sender as Button;
            senderButton.IsEnabled = false;

            // Clear previous returned file name, if it exists, between iterations of this scenario
            Text = "";

            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // See the sample code below for how to make the window accessible from the App class.
            var window = App.m_Window;

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");


            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                Text = "Picked file: " + file.Name;
                await Image_Load(file);
            }
            else
            {
                Text = "Operation cancelled.";
            }

            senderButton.IsEnabled = true;
        }

        async Task Image_Load(StorageFile? file)
        {
            using var stream = await file.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            SoftwareBitmap displayableImage = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var imageLayer = new ImageLayer(displayableImage, file.Name, decoder, Delete, Render);
            LayersVM.Add(imageLayer);
            Render();
        }

        private async void Render()
        {
            ViewPortVM.Render(LayersVM);

            if (ViewPortVM.View != null)
            {
                CurviesVM.ViewReference = CopyWriteableBitmap(ViewPortVM.View);
                BinarizationVM.SetReference(CurviesVM.ViewReference);
            }
            else
            {
                CurviesVM.ViewReference = null;
                BinarizationVM.SetReference(null);
            }
            
        }

        public void Delete(object sender, ImageLayer imageLayer)
        {
            LayersVM.Delete(sender, imageLayer);
            if (LayersVM.Count() != 0)
                Render();
            else
            {
                ViewPortVM.View = null;
                CurviesVM.ViewReference = null;
            }
        }

        public void RightPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var rb = sender as RadioButtons;
            if (rb != null)
            {
                string selected = rb.SelectedItem as string;
                switch (selected)
                {
                    case "Слои":
                        LayersVM.Visibility = Visibility.Visible;
                        CurviesVM.Visibility = Visibility.Collapsed;
                        BinarizationVM.Visibility = Visibility.Collapsed;
                        Render();
                        break;
                    case "Кривые":
                        LayersVM.Visibility = Visibility.Collapsed;
                        CurviesVM.Visibility = Visibility.Visible;
                        BinarizationVM.Visibility = Visibility.Collapsed;
                        CurviesVM.RenderViewPort();
                        break;
                    case "Бинаризация":
                        LayersVM.Visibility = Visibility.Collapsed;
                        CurviesVM.Visibility = Visibility.Collapsed;
                        BinarizationVM.Visibility = Visibility.Visible;
                        ViewPortVM.View = BinarizationVM.ViewReference;
                        break;
                }
            }
        }

        private static WriteableBitmap? CopyWriteableBitmap(WriteableBitmap? source)
        {
            if (source == null) return null;

            var copy = new WriteableBitmap(source.PixelWidth, source.PixelHeight);
            using (var srcStream = source.PixelBuffer.AsStream())
            using (var destStream = copy.PixelBuffer.AsStream())
            {
                srcStream.CopyTo(destStream);
            }
            return copy;
        }
    }
}
