using Client_UWP.Models;
using Client_UWP.Pages.SettingsConnection;
using Client_UWP.Pages.SettingsDebug;
using Client_UWP.Pages.SettingsDevices;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Client_UWP.Pages.SettingsAccount
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsAccountEditorPage : Page
    {
        private ObservableCollection<AccountModel> _accountsList { get; set; } = new ObservableCollection<AccountModel>();

        private LocalSettings _localSettings = new LocalSettings();

        public SettingsAccountEditorPage()
        {
            InitializeComponent();

            GoToSettingsAccountPage.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));

            ConnectionSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ObservableCollection<AccountModel> list = _localSettings.DeserializeAccountsList();

            foreach (AccountModel acc in list)
                _accountsList.Add(acc);

            if (e.Parameter != null)
            {
                AccountModel account = (AccountModel)e.Parameter;

                // Add new Account to AccountsList
                _accountsList.Add(account);

                // Save AccountsList
                _localSettings.SerializeAccountsList(_accountsList);

                tbAccountName.Text = account.AccountName;
                tbServiceUri.Text = account.ServiceUri;
                tbIdentityUri.Text = account.IdentityUri;
                btnAdd.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;

                btnDelete.Click += (sender, args) =>
                {
                    // Remove Account from AccountsList
                    _accountsList.Remove(account);

                    // Save AccountsList
                    _localSettings.SerializeAccountsList(_accountsList);

                    // Remove selected account
                    _localSettings.SerializeAccountsList(null);

                    Frame.Navigate(typeof(SettingsAccountPage));
                };

                btnSave.Click += (sender, args) =>
                {
                    // Remove Account from AccountsList
                    _accountsList.Remove(account);

                    // Add Account to AccountsList
                    _accountsList.Add(new AccountModel
                    {
                        AccountName = tbAccountName.Text,
                        ServiceUri = tbServiceUri.Text,
                        IdentityUri = tbIdentityUri.Text
                    });

                    // Save AccountsList
                    _localSettings.SerializeAccountsList(_accountsList);

                    Frame.Navigate(typeof(SettingsAccountPage));
                };
            }

            btnAdd.Click += (sender, args) =>
            {
                AccountModel accountModel = new AccountModel
                {
                    AccountName = tbAccountName.Text,
                    ServiceUri = tbServiceUri.Text,
                    IdentityUri = tbIdentityUri.Text
                };

                // Add new Account to AccountsList
                _accountsList.Add(accountModel);

                // Save AccountsList
                _localSettings.SerializeAccountsList(_accountsList);

                Frame.Navigate(typeof(SettingsAccountPage), accountModel);
            };
        }
    }
}
