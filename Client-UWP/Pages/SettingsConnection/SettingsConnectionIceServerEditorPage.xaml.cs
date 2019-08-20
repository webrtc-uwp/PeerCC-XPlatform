using Client_UWP.Models;
using Client_UWP.Pages.SettingsAccount;
using Client_UWP.Pages.SettingsDebug;
using Client_UWP.Pages.SettingsDevices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Client_UWP.Pages.SettingsConnection
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsConnectionIceServerEditorPage : Page
    {
        private ObservableCollection<IceServerModel> _iceServersList { get; set; } = new ObservableCollection<IceServerModel>();

        private LocalSettings _localSettings = new LocalSettings();

        public SettingsConnectionIceServerEditorPage()
        {
            InitializeComponent();

            GoToSettingsConnectionPage.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));
            AccountSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));
            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));
            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));
        }

        private List<string> ListUrls = new List<string>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ObservableCollection<IceServerModel> list = _localSettings.DeserializeIceServersList();

            foreach (IceServerModel ice in list)
                _iceServersList.Add(ice);

            if (e.Parameter != null)
            {
                IceServerModel iceServer = (IceServerModel)e.Parameter;

                // Add new IceServer to IceServersList
                _iceServersList.Add(iceServer);

                _localSettings.SerializeIceServersList(_iceServersList);

                foreach (string url in iceServer.Urls)
                {
                    listIceServers.Items.Add(url);
                    ListUrls.Add(url);
                }

                tbUsername.Text = iceServer.Username != null ? iceServer.Username : string.Empty;
                pbCredential.Password = iceServer.Credential != null ? iceServer.Credential : string.Empty;
                btnAdd.Visibility = Visibility.Collapsed;
                btnSave.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;

                btnDelete.Click += (sender, args) =>
                {
                    // Remove IceServer from IceServersList
                    _iceServersList.Remove(iceServer);

                    _localSettings.SerializeIceServersList(_iceServersList);

                    Frame.Navigate(typeof(SettingsConnectionPage));
                };

                btnSave.Click += (sender, args) =>
                {
                    // Remove IceServer from IceServersList
                    _iceServersList.Remove(iceServer);

                    // Add new IceServer to IceServersList
                    _iceServersList.Add(new IceServerModel
                    {
                        Urls = ListUrls,
                        Username = tbUsername.Text,
                        Credential = pbCredential.Password
                    });

                    _localSettings.SerializeIceServersList(_iceServersList);

                    Frame.Navigate(typeof(SettingsConnectionPage));
                };
            }

            btnAdd.Click += (sender, args) => 
            {
                // Add new IceServer to IceServersList
                _iceServersList.Add(new IceServerModel
                {
                    Urls = ListUrls,
                    Username = tbUsername.Text,
                    Credential = pbCredential.Password
                });

                _localSettings.SerializeIceServersList(_iceServersList);

                Frame.Navigate(typeof(SettingsConnectionPage));
            };

            btnAddUrl.Click += (sender, args) =>
            {
                listIceServers.Items.Add(tbServerUrl.Text);

                ListUrls.Add(tbServerUrl.Text);

                tbServerUrl.Text = "";
            };
        }
    }
}
