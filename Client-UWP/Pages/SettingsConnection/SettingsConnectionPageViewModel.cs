using Client_UWP.Models;
using Client_UWP.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Client_UWP.Pages.SettingsConnection
{
    public class SettingsConnectionPageViewModel : ViewModelBase
    {
        public static ObservableCollection<IceServer> _iceServersList { get; set; }

        public static ObservableCollection<IceServer> AddDefaultIceServers()
        {
            List<IceServer> list = DefaultSettings.IceServersList;

            foreach (IceServer ice in list)
                _iceServersList.Add(ice);

            return _iceServersList;
        }
    }
}
