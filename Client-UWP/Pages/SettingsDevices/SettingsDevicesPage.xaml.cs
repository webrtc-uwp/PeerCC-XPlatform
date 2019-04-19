using Client_UWP.Pages.SettingsAccount;
using Client_UWP.Pages.SettingsConnection;
using Client_UWP.Pages.SettingsDebug;
using GuiCore;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Client_UWP.Pages.SettingsDevices
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsDevicesPage : Page
    {
        private SettingsDevicesPageViewModel ViewModel { get; set; }

        public SettingsDevicesPage()
        {
            InitializeComponent();

            ViewModel = new SettingsDevicesPageViewModel();

            InitView();
        }

        private void InitView()
        {
            GoToMainPage.Click += (sender, args) => Frame.Navigate(typeof(MainPage));

            AccountSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));

            ConnectionSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));

            cbCamera.ItemsSource = Devices.Instance.CamerasList;
            cbCamera.SelectedIndex = 0;

            List<string> acList = new List<string>();

            foreach (var ac in ViewModel.AudioCodecsList)
                acList.Add(ac.Name);

            cbAudioCodec.ItemsSource = acList;
            cbAudioCodec.SelectedIndex = 0;

            List<string> vcList = new List<string>();

            foreach (var vc in ViewModel.VideoCodecsList)
                vcList.Add(vc.Name);

            cbVideoCodec.ItemsSource = vcList;
            cbVideoCodec.SelectedIndex = 0;
        }
    }
}
