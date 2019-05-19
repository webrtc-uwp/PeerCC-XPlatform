using Client_UWP.Models;
using GuiCore.Utilities;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace Client_UWP.Pages
{
    public class LocalSettings
    {
        public ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public void SerializeIceServersList(ObservableCollection<IceServerModel> IceServersList)
        {
            localSettings.Values["IceServersList"] =
                XmlSerialization<ObservableCollection<IceServerModel>>.Serialize(IceServersList);
        }

        public ObservableCollection<IceServerModel> DeserializeIceServersList() =>
            XmlSerialization<ObservableCollection<IceServerModel>>.Deserialize((string)localSettings.Values["IceServersList"]);
    }
}
