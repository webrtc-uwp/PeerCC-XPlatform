using Client_UWP.Models;
using Client_UWP.Pages.Main;
using Client_UWP.Pages.SettingsAccount;
using Client_UWP.Pages.SettingsDebug;
using Client_UWP.Pages.SettingsDevices;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Client_UWP.Pages.SettingsConnection
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsConnectionPage : Page
    {
        private ObservableCollection<IceServerModel> _iceServersList { get; set; } = new ObservableCollection<IceServerModel>();

        private LocalSettings _localSettings = new LocalSettings();

        public SettingsConnectionPage()
        {
            InitializeComponent();

            InitView();
        }

        private void InitView()
        {
            ObservableCollection<IceServerModel> list = _localSettings.DeserializeIceServersList();

            foreach (IceServerModel ice in list)
                _iceServersList.Add(ice);

            GoToMainPage.Click += (sender, args) => Frame.Navigate(typeof(MainPage));

            AccountSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));

            DevicesSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDevicesPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));

            AddServer.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionIceServerEditorPage));

            EditServer.Click += async (sender, args) => 
            {
                if (IceServersListView.SelectedIndex == -1)
                {
                    await new MessageDialog("Please select Ice Server you want to edit.").ShowAsync();
                    return;
                }

                IceServerModel iceServer = IceServersListView.SelectedItem as IceServerModel;
                if (iceServer == null) return;

                // Remove IceServer from IceServersList
                _iceServersList.Remove(iceServer);

                _localSettings.SerializeIceServersList(_iceServersList);

                Frame.Navigate(typeof(SettingsConnectionIceServerEditorPage), iceServer);
            };

            IceServersListView.Tapped += IceServersListView_Tapped;

            RemoveServer.Click += async (sender, args) =>
            {
                if (IceServersListView.SelectedIndex == -1)
                {
                    await new MessageDialog("Please select Ice Server you want to remove.").ShowAsync();
                    return;
                }

                IceServerModel iceServer = IceServersListView.SelectedItem as IceServerModel;
                if (iceServer == null) return;

                // Remove IceServer from IceServersList
                _iceServersList.Remove(iceServer);

                _localSettings.SerializeIceServersList(_iceServersList);
            };
        }

        private void IceServersListView_Tapped(object sender, TappedRoutedEventArgs e) =>
            IceServersListView.SelectedItem = (IceServerModel)((FrameworkElement)e.OriginalSource).DataContext;
    }
}
