using ImageLinker2.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;
namespace ImageLinker2.ViewModel
{
    public class LayersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ImageLayer> _layers;
        public ObservableCollection<ImageLayer> layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                OnPropertyChanged(nameof(ImageLayer));
            }
        }

        public LayersViewModel()
        {
            //_layerModel = new ViewLayers();
            _layers = new ObservableCollection<ImageLayer>();
            layers = new ObservableCollection<ImageLayer>();
        }

        public void Add(ImageLayer layer)
        {
            layers.Add(layer);
        }

        public void Delete(object sender, ImageLayer imageLayer)
        {
            imageLayer.Dispose();
            layers.Remove(imageLayer);
        }

        public ImageLayer GetLayer(int index)
        {
            return _layers[index];
        }

        public int Count()
        {
            return _layers.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
