using System.ComponentModel;

namespace Client_UWP.Pages
{
    /// <summary>
    /// The BindableBase class of the MVVM pattern.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="properetyName">Name of the property used to notify listeners.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler changedEventHandler = PropertyChanged;

            if (changedEventHandler == null) return;

            changedEventHandler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
