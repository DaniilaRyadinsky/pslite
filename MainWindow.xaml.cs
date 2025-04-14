using Microsoft.UI.Xaml;
using ImageLinker2.ViewModel;
using System.Threading.Tasks;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ImageLinker2
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public MainWindowViewModel _viewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            _viewModel = new MainWindowViewModel();
        }
    }
}