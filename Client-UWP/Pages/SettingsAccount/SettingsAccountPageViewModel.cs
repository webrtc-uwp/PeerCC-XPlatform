using Client_UWP.Models;
using Client_UWP.Utilities;
using GuiCore.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;

namespace Client_UWP.Pages.SettingsAccount
{
    public class SettingsAccountPageViewModel : ViewModelBase
    {
        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public SettingsAccountPageViewModel()
        {
            try
            {
                if (localSettings.Values["AccountsList"] != null)
                {
                    if (XmlSerialization<ObservableCollection<AccountModel>>
                    .Deserialize((string)localSettings.Values["AccountsList"]).Any())
                    {
                        ObservableCollection<AccountModel> list =
                        XmlSerialization<ObservableCollection<AccountModel>>
                        .Deserialize((string)localSettings.Values["AccountsList"]);

                        foreach (AccountModel a in list)
                            AccountsList.Add(a);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] SettingsAccountPageViewModel message: " + ex.Message);
            }
        }

        public ObservableCollection<AccountModel> AccountsList { get; set; } =
            new ObservableCollection<AccountModel>();
    }
}
