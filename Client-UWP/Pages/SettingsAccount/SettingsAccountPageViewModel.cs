using Client_UWP.Controllers;
using Client_UWP.Models;
using Client_UWP.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Client_UWP.Pages.SettingsAccount
{
    public class SettingsAccountPageViewModel : ViewModelBase
    {
        public SettingsAccountPageViewModel()
        {
            try
            {
                if (SettingsController.Instance.localSettings.Values["AccountsList"] != null)
                {
                    if (XmlSerialization<ObservableCollection<Account>>
                    .Deserialize((string)SettingsController.Instance.localSettings.Values["AccountsList"]).Any())
                    {
                        ObservableCollection<Account> list =
                        XmlSerialization<ObservableCollection<Account>>
                        .Deserialize((string)SettingsController.Instance.localSettings.Values["AccountsList"]);

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
