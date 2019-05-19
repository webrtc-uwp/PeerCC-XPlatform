using Client_UWP.Models;
using Client_UWP.Pages.SettingsConnection;
using Client_UWP.Pages.SettingsDebug;
using Client_UWP.Pages.SettingsDevices;
using GuiCore.Utilities;
using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Client_UWP.Pages.SettingsAccount
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsAccountPage : Page
    {
        private ObservableCollection<AccountModel> _accountsList { get; set; } = new ObservableCollection<AccountModel>();

        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public SettingsAccountPage()
        {
            InitializeComponent();

            InitView();
        }

        private void InitView()
        {
            AccountsListView.Loaded += (sender, args) =>
            {
                ObservableCollection<AccountModel> list =
                    XmlSerialization<ObservableCollection<AccountModel>>.Deserialize((string)localSettings.Values["AccountsList"]);

                foreach (AccountModel acc in list)
                    _accountsList.Add(acc);

                AccountModel selectedAccount = XmlSerialization<AccountModel>.Deserialize((string)localSettings.Values["SelectedAccount"]);

                for (int i = 0; i < AccountsListView.Items.Count; i++)
                {
                    AccountModel item = (AccountModel)AccountsListView.Items[i];

                    if (selectedAccount != null)
                        if (selectedAccount.AccountName == item.AccountName)
                            if (selectedAccount.ServiceUri == item.ServiceUri)
                                AccountsListView.SelectedIndex = i;
                }
            };

            GoToMainPage.Click += (sender, args) => Frame.Navigate(typeof(MainPage));

            ConnectionSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));

            AccountsListView.Tapped += AccountsListView_Tapped;

            AddAccount.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountEditorPage));

            EditAccount.Click += async (sender, args) =>
            {
                if (AccountsListView.SelectedIndex == -1)
                {
                    await new MessageDialog("Please select Account you want to edit").ShowAsync();
                    return;
                }

                AccountModel account = AccountsListView.SelectedItem as AccountModel;
                if (account == null) return;

                // Remove Account from AccountsList
                _accountsList.Remove(account);

                // Save AccountsList
                localSettings.Values["AccountsList"] =
                    XmlSerialization<ObservableCollection<AccountModel>>.Serialize(_accountsList);

                Frame.Navigate(typeof(SettingsAccountEditorPage), account);
            };

            RemoveAccount.Click += async (sender, args) => 
            {
                if (AccountsListView.SelectedIndex == -1)
                {
                    await new MessageDialog("Please select Account you want to remove.").ShowAsync();
                    return;
                }

                AccountModel account = AccountsListView.SelectedItem as AccountModel;
                if (account == null) return;

                // Remove Account form AccountsList
                _accountsList.Remove(account);

                // Save AccoutsList
                localSettings.Values["AccountsList"] =
                    XmlSerialization<ObservableCollection<AccountModel>>.Serialize(_accountsList);

                // Remove selected account
                localSettings.Values["SelectedAccount"] = null;
            };
        }

        private void AccountsListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AccountsListView.SelectedItem = (AccountModel)((FrameworkElement)e.OriginalSource).DataContext;

            localSettings.Values["SelectedAccount"] =
                XmlSerialization<AccountModel>.Serialize((AccountModel)AccountsListView.SelectedItem);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AccountModel newAccount = (AccountModel)e.Parameter;

            if (newAccount != null)
            {
                localSettings.Values["SelectedAccount"] =
                    XmlSerialization<AccountModel>.Serialize(newAccount);

                AccountsListView.SelectedItem = AccountsListView.Items.Count - 1;
            }
        }
    }
}
