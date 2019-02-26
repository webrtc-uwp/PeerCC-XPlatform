using Client_UWP.Controllers;
using Client_UWP.Models;
using Client_UWP.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace Client_UWP.Pages.SettingsConnection
{
    public class SettingsConnectionPageViewModel : ViewModelBase
    {
        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public SettingsConnectionPageViewModel()
        {
            if (XmlSerialization<ObservableCollection<IceServer>>
                .Deserialize((string)localSettings.Values["IceServersList"]) == null)
            {
                AddDefaultIceServers(IceServersList);
                localSettings.Values["IceServersList"] = 
                    XmlSerialization<ObservableCollection<IceServer>>.Serialize(IceServersList);
            }
            else
            {
                ObservableCollection<IceServer> list = 
                    XmlSerialization<ObservableCollection<IceServer>>
                    .Deserialize((string)localSettings.Values["IceServersList"]);

                foreach (IceServer ice in list)
                    IceServersList.Add(ice);
            }
        }

        public ObservableCollection<IceServer> IceServersList { get; set; } = new ObservableCollection<IceServer>();

        public static ObservableCollection<IceServer> AddDefaultIceServers(ObservableCollection<IceServer> IceServersList)
        {
            List<IceServer> list = DefaultSettings.IceServersList;

            foreach (IceServer ice in list)
                IceServersList.Add(ice);

            return IceServersList;
        }
    }
}
