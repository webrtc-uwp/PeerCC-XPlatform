using Client_UWP.Models;
using GuiCore.Utilities;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace Client_UWP.Pages
{
    public class LocalSettings
    {
        public ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public void SerializeIceServersList(ObservableCollection<IceServerModel> iceServersList)
        {
            localSettings.Values["IceServersList"] =
                XmlSerialization<ObservableCollection<IceServerModel>>.Serialize(iceServersList);
        }

        public ObservableCollection<IceServerModel> DeserializeIceServersList() =>
            XmlSerialization<ObservableCollection<IceServerModel>>.Deserialize((string)localSettings.Values["IceServersList"]);

        public void SerializeAccountsList(ObservableCollection<AccountModel> accountsList)
        {
            localSettings.Values["AccountsList"] =
                    XmlSerialization<ObservableCollection<AccountModel>>.Serialize(accountsList);
        }

        public ObservableCollection<AccountModel> DeserializeAccountsList() =>
            XmlSerialization<ObservableCollection<AccountModel>>.Deserialize((string)localSettings.Values["AccountsList"]);

        public void SerializeSelectedAccount(AccountModel account)
        {
            localSettings.Values["SelectedAccount"] = XmlSerialization<AccountModel>.Serialize(account);
        }

        public AccountModel DeserializeSelectedAccount() =>
            XmlSerialization<AccountModel>.Deserialize((string)localSettings.Values["SelectedAccount"]);
    }
}
