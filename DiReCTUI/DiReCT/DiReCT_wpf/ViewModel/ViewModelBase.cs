using System.ComponentModel;

namespace DiReCT_wpf.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public abstract string Name { get; }

        internal void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
