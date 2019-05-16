using Client_UWP.Models;
using Client_UWP.Utilities;
using ClientCore.Call;
using GuiCore;
using GuiCore.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;

namespace Client_UWP.Pages.SettingsConnection
{
    public class SettingsConnectionPageViewModel : ViewModelBase
    {
        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public ObservableCollection<IceServerModel> IceServersList { get; set; } = new ObservableCollection<IceServerModel>();

        

        public SettingsConnectionPageViewModel()
        {
            try
            {
                ObservableCollection<IceServerModel> list =
                    XmlSerialization<ObservableCollection<IceServerModel>>
                    .Deserialize((string)localSettings.Values["IceServersList"]);

                foreach (IceServerModel ice in list)
                    IceServersList.Add(ice);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] SettingsConnectionPageViewModel message: " + ex.Message);
            }
        }
    }
}
