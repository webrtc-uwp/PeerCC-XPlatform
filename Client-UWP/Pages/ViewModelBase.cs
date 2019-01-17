using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client_UWP.Pages
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the ProperyChanged event, passing the name of the property whose value has changed.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
