using Client_UWP.Models;
using Client_UWP.Utilities;
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
                    if (XmlSerialization<ObservableCollection<Account>>
                    .Deserialize((string)localSettings.Values["AccountsList"]).Any())
                    {
                        ObservableCollection<Account> list =
                        XmlSerialization<ObservableCollection<Account>>
                        .Deserialize((string)localSettings.Values["AccountsList"]);

                        foreach (Account a in list)
                            AccountsList.Add(a);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] SettingsAccountPageViewModel message: " + ex.Message);
            }
        }

        public ObservableCollection<Account> AccountsList { get; set; } =
            new ObservableCollection<Account>();
    }
}
