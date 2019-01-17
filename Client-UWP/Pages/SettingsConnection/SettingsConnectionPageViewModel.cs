using Client_UWP.Models;
using Client_UWP.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Client_UWP.Pages.SettingsConnection
{
    public class SettingsConnectionPageViewModel : ViewModelBase
    {
        public SettingsConnectionPageViewModel()
        {
            AddDefaultIceServers(IceServersList);
        }

        private ObservableCollection<IceServer> _iceServersList = new ObservableCollection<IceServer>();
        public ObservableCollection<IceServer> IceServersList { get { return _iceServersList; } }

        public static ObservableCollection<IceServer> AddDefaultIceServers(ObservableCollection<IceServer> IceServersList)
        {
            List<IceServer> list = DefaultSettings.IceServersList;

            foreach (IceServer ice in list)
                IceServersList.Add(ice);

            return IceServersList;
        }
    }
}
