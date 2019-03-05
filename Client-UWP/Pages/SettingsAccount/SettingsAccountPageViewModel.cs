using Client_UWP.Controllers;
using Client_UWP.Models;
using Client_UWP.Utilities;
using System.Collections.ObjectModel;
using System.Linq;

namespace Client_UWP.Pages.SettingsAccount
{
    public class SettingsAccountPageViewModel : ViewModelBase
    {
        public SettingsAccountPageViewModel()
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

        public ObservableCollection<Account> AccountsList { get; set; } =
            new ObservableCollection<Account>();
    }
}
