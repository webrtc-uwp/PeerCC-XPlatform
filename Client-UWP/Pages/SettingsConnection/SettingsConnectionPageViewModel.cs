using Client_UWP.Models;
using Client_UWP.Utilities;
using ClientCore.Call;
using GuiCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using WebRtcAdapter;
using Windows.Storage;

namespace Client_UWP.Pages.SettingsConnection
{
    public class SettingsConnectionPageViewModel : ViewModelBase
    {
        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public SettingsConnectionPageViewModel()
        {
            try
            {
                if (localSettings.Values["IceServersList"] != null)
                {
                    if (!(XmlSerialization<ObservableCollection<IceServerModel>>
                .Deserialize((string)localSettings.Values["IceServersList"])).Any())
                    {
                        ObservableCollection<IceServerModel> iceServersList = AddDefaultIceServers(IceServersList);

                        localSettings.Values["IceServersList"] =
                            XmlSerialization<ObservableCollection<IceServerModel>>.Serialize(iceServersList);

                        ObservableCollection<IceServerModel> list =
                            XmlSerialization<ObservableCollection<IceServerModel>>
                            .Deserialize((string)localSettings.Values["IceServersList"]);

                        List<IceServer> iceServerList = new List<IceServer>();

                        foreach (IceServerModel ice in list)
                        {
                            List<string> urls = new List<string>();
                            urls.Add(ice.Url);
                            IceServer iceServer = new IceServer();
                            iceServer.Urls = urls;
                            iceServer.Username = ice.Username;
                            iceServer.Credential = ice.Credential;
                            iceServerList.Add(iceServer);
                        }

                        RtcController.Instance.ConfigureIceServers(iceServerList);
                    }
                    else
                    {
                        ObservableCollection<IceServerModel> list =
                            XmlSerialization<ObservableCollection<IceServerModel>>
                            .Deserialize((string)localSettings.Values["IceServersList"]);

                        foreach (IceServerModel ice in list)
                            IceServersList.Add(ice);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] SettingsConnectionPageViewModel message: " + ex.Message);
            }
        }

        public ObservableCollection<IceServerModel> IceServersList { get; set; } = new ObservableCollection<IceServerModel>();

        public static ObservableCollection<IceServerModel> AddDefaultIceServers(ObservableCollection<IceServerModel> IceServersList)
        {
            List<IceServerModel> list = DefaultSettings.IceServersList;

            foreach (IceServerModel ice in list)
                IceServersList.Add(ice);

            return IceServersList;
        }
    }
}
