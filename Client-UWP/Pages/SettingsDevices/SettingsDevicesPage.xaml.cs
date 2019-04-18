using Client_UWP.Pages.SettingsAccount;
using Client_UWP.Pages.SettingsConnection;
using Client_UWP.Pages.SettingsDebug;
using GuiCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Client_UWP.Pages.SettingsDevices
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsDevicesPage : Page
    {
        private SettingsDevicesPageViewModel _settingsDevicesPageViewModel;

        public SettingsDevicesPage()
        {
            InitializeComponent();

            //ViewModel = new SettingsDevicesPageViewModel();

            GoToMainPage.Click += (sender, args) => Frame.Navigate(typeof(MainPage));

            AccountSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));

            ConnectionSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));

            
        }

        /// <summary>
        /// See Page.OnNavigatedTo()
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _settingsDevicesPageViewModel = (SettingsDevicesPageViewModel)e.Parameter;
            DataContext = _settingsDevicesPageViewModel;

            List<string> camerasList = new List<string>();

            IList<GuiLogic.MediaDeviceModel> videoDevices;

            Task.Run(async () =>
            {
                videoDevices = await GuiLogic.GetVideoCaptureDevices();

                foreach (GuiLogic.MediaDeviceModel videoCaptureDevice in videoDevices)
                    camerasList.Add(videoCaptureDevice.Name);
            });

            cbCamera.ItemsSource = camerasList;
            cbCamera.SelectedIndex = 0;

            //List<string> acList = new List<string>();

            //foreach (var ac in _settingsDevicesPageViewModel.AudioCodecsList)
            //    acList.Add(ac.Name);

            //cbAudioCodec.ItemsSource = acList;
            //cbAudioCodec.SelectedIndex = 0;

            //List<string> vcList = new List<string>();

            //foreach (var vc in _settingsDevicesPageViewModel.VideoCodecsList)
            //    vcList.Add(vc.Name);

            //cbVideoCodec.ItemsSource = vcList;
            //cbVideoCodec.SelectedIndex = 0;
        }

        //SettingsDevicesPageViewModel ViewModel { get; set; }
    }
}
