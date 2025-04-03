using ImageLinker2.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Windows.Graphics.Imaging;


namespace ImageLinker2.ViewModel
{
    public class ViewPortViewModel : BaseViewModel
    {
        private WriteableBitmap? _view;
        public WriteableBitmap? View
        {
            get { return _view; }
            set
            {
                _view = value;
                OnPropertyChanged(nameof(View));
            }
        }

        public ViewPortViewModel(){}

        public async void Render(LayersViewModel LayersVM) // нужно еще подумать
        {
            if (LayersVM.Count() > 0)
            {
                if (View== null)
                    View = await ViewPort.Copy(LayersVM.GetLayer(0).Referense);
                else
                {
                    var layer = LayersVM.GetLayer(0);
                    View = await ViewPort.UpdateViewPort(layer.Referense, layer.GetR(), layer.GetG(), layer.GetB(), layer.GetOpasity());
                }
            }
            for (var i = 1; i < LayersVM.Count(); i++)
            {
                var layer = LayersVM.GetLayer(i);
                View = await ViewPort.BuildViewPort(View, layer.Referense, layer.GetMode(), layer.GetR(), layer.GetG(), layer.GetB(), layer.GetOpasity());
            }
        }

        public async void Render(WriteableBitmap? ViewReference, Dictionary<int, int> map)
        {
            View = await ViewPort.UpdateViewPort(ViewReference, map);
        }
        
    }
}
