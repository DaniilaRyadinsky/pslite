using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLinker2.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public enum Mode
        {
            Gavr,
            Otsu,
            Nibl,
            Sauv,
            Wulf,
            Rot
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
