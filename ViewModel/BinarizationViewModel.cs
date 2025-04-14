using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageLinker2.Models;

namespace ImageLinker2.ViewModel
{
    public class BinarizationViewModel : BaseViewModel
    {

        private Visibility _Visibility = Visibility.Collapsed;
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

        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        Mode _mode = Mode.Gavr;

        public int WindowSize = 3;
        public double a = 0.5;
        public double k = 0.2;

        public WriteableBitmap? ViewReference;
        public ViewPortViewModel ViewPortVM;

        public BinarizationViewModel(ViewPortViewModel viewPortVM)
        {
            ViewReference = null;
            ViewPortVM = viewPortVM;
        }

        public void Mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var rb = sender as RadioButtons;
            if (rb != null)
            {
                string selected = rb.SelectedItem as string;
                switch (selected)
                {
                    case "Критерий Гаврилова":
                        _mode = Mode.Gavr;
                        IsEnabled = false;
                        break;
                    case "Критерий Отсу":
                        _mode = Mode.Otsu;
                        IsEnabled = false;
                        break;
                    case "Критерий Ниблека":
                        _mode = Mode.Nibl;
                        IsEnabled = true;
                        break;
                    case "Критерий Сауволы":
                        _mode = Mode.Sauv;
                        IsEnabled = true;
                        break;
                    case "Критерий Кристиана Вульфа":
                        _mode = Mode.Wulf;
                        IsEnabled = true;
                        break;
                    case "Критерий Брэдли-Рота":
                        _mode = Mode.Rot;
                        IsEnabled = true;
                        break;
                }
            }
        }

        public void WindowSize_Changed(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox != null)
            {
                if (textbox.Text.Length > 0)
                    WindowSize = Int32.Parse(textbox.Text);
            }
        }

        public void A_Changed(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox != null)
            {
                if (textbox.Text.Length > 0)
                    a = Double.Parse(textbox.Text);
            }
        }

        public void K_Changed(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox != null)
            {
                if (textbox.Text.Length > 0)
                    k = Double.Parse(textbox.Text);
            }
        }

        public async void Render_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null)
            {
                btn.IsEnabled = false;
                WriteableBitmap? result;
                switch (_mode)
                {
                    case Mode.Gavr:
                        result = await BinarizationModel.BinarizationGavr(ViewReference);
                        break;
                    case Mode.Otsu:
                        result = await BinarizationModel.BinarizationOtsu(ViewReference);
                        break;
                    case Mode.Sauv:
                        result = await BinarizationModel.BinarizationSauv(ViewReference, WindowSize, k);
                        break;
                    case Mode.Nibl:
                        result = await BinarizationModel.BinarizationNibl(ViewReference, WindowSize, k);
                        break;
                    case Mode.Wulf:
                        result = await BinarizationModel.BinarizationWulf(ViewReference, WindowSize, a);
                        break;
                    //case Mode.Rot:
                    //    result = await BinarizationModel.BinarizationRot(ViewReference);
                    //    break;
                    default: result = null; break;

                }
                ViewPortVM.View = result;
                btn.IsEnabled = true;
            }
        }

        public async void SetReference(WriteableBitmap? _reference)
        {
            ViewReference = await BinarizationModel.MakeGradGray(_reference);
        }
    }
}
