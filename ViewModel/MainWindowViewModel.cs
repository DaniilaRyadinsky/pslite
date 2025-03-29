using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;
using ImageLinker2.Models;

namespace ImageLinker2.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public LayersViewModel LayersVM { get; }
        public ViewPortViewModel ViewPortVM { get; }

        private SoftwareBitmapSource? _viewportSource;
        public SoftwareBitmapSource? ViewportSource
        {
            get { return _viewportSource; }
            set
            {
                _viewportSource = value;
                OnPropertyChanged(nameof(ViewportSource));
            }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }


        public MainWindowViewModel(LayersViewModel _LayersVM)
        {
            LayersVM = _LayersVM;
            ViewPortVM = new ViewPortViewModel();
            _viewportSource = new();
            ViewportSource = new();
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
                await image_Load(file);
            }
            else
            {
                Text = "Operation cancelled.";
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

            var imageLayer = new ImageLayer(displayableImage, file.Name, decoder, Delete, Render);
            LayersVM.Add(imageLayer);

            Render();
        }

        public async void Render()
        {
            ViewPortVM.RenderLayers(LayersVM);

            ViewportSource = null;
            SoftwareBitmapSource sourse = new();
            await sourse.SetBitmapAsync(ViewPortVM.View);
            ViewportSource = sourse;
        }

        public void Delete(object sender, ImageLayer imageLayer)
        {
            LayersVM.Delete(sender, imageLayer);
            if (LayersVM.Count() == 0)
            {
                ViewportSource?.Dispose();
                _viewportSource?.Dispose();
                ViewportSource = null;
                ViewPortVM.Dispose();
            }
            else
                Render();

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
