using ImageLinker2.Models;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
namespace ImageLinker2.ViewModel
{
    public class LayersViewModel : BaseViewModel
    {
        private Visibility _Visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get => _Visibility;
            set
            {
                if (_Visibility != value)
                {
                    _Visibility = value;
                    OnPropertyChanged(nameof(Visibility));
                }
            }
        }

        private ObservableCollection<ImageLayer> _layers;
        public ObservableCollection<ImageLayer> layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                OnPropertyChanged(nameof(layers));
            }
        }

        public LayersViewModel()
        {
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
            if (index < layers.Count)
                return layers[index];
            return null;
        }

        public int Count()
        {
            return _layers.Count;
        }

    }
}
