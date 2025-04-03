using Microsoft.UI.Xaml.Controls;
using ImageLinker2.ViewModel;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ImageLinker2
{
    public sealed partial class Layers : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(
            "ViewModel",
            typeof(LayersViewModel),
            typeof(Layers),
            new PropertyMetadata(null));

        public LayersViewModel ViewModel
        {
            get => (LayersViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public Layers()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

    }

}


