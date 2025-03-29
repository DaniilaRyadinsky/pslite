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
using ImageLinker2.ViewModel;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ImageLinker2
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        //public Layers layers;
        //public ViewPort ViewPort;

        public MainWindowViewModel _viewModel;
        public Layers layers;

        public MainWindow()
        {
            this.InitializeComponent();
            layers = new Layers();
            _viewModel = new MainWindowViewModel(layers._viewModel);
            RightPanel.Children.Add(layers);
        }

    }
}