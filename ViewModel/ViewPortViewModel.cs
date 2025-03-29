using ImageLinker2.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace ImageLinker2.ViewModel
{
    public class ViewPortViewModel : INotifyPropertyChanged
    {
        //private readonly ViewPort _viewPort = new();
        private SoftwareBitmap? _view;
        public SoftwareBitmap? View
        {
            get { return _view; }
            set
            {
                _view = value;
                OnPropertyChanged(nameof(View));
            }
        }

        public ViewPortViewModel()
        {
            //_view = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 10, 10, BitmapAlphaMode.Premultiplied);
            //View = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 10, 10, BitmapAlphaMode.Premultiplied);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async void RenderLayers(LayersViewModel LayersVM) // нужно еще подумать
        {
            if (_view == null)
            {
                this.Dispose(); 
                View = SoftwareBitmap.Copy(LayersVM.GetLayer(0).Referense);
            }
            else if (LayersVM.Count() != 0)
            {
                this.Dispose(); 
                var layer = LayersVM.GetLayer(0);
                View = await ViewPort.UpdateViewPort(layer.Referense, layer.GetR(), layer.GetG(), layer.GetB(), layer.GetOpasity());
            }
            for (var i = 1; i < LayersVM.Count(); i++)
            {
                var layer = LayersVM.GetLayer(i);
                View = await ViewPort.BuildViewPort(View, layer.Referense, layer.GetMode(), layer.GetR(), layer.GetG(), layer.GetB(), layer.GetOpasity());
            }
        }

        public void Dispose()
        {
            _view?.Dispose();
            _view = null;
            View?.Dispose();
            View = null;
        }
    }
}
