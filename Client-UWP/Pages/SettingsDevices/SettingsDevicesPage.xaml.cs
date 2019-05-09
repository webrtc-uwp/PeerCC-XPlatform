using Client_UWP.Pages.SettingsAccount;
using Client_UWP.Pages.SettingsConnection;
using Client_UWP.Pages.SettingsDebug;
using GuiCore;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Client_UWP.Pages.SettingsDevices
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsDevicesPage : Page
    {
        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        private SettingsDevicesPageViewModel ViewModel { get; set; }

        private List<string> CamerasList = new List<string>();
        private List<string> MicrophonesList = new List<string>();
        private List<string> SpeakersList = new List<string>();

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

            foreach (var videoDevice in Devices.Instance.VideoDevicesList)
                CamerasList.Add(videoDevice.Name);

            cbCamera.ItemsSource = CamerasList;

            cbCamera.SelectionChanged += CbCamera_SelectionChanged;

            ItemCollection cameras = cbCamera.Items;

            for (int i = 0; i < cameras.Count; i++)
                if (cameras[i].ToString() == localSettings.Values["SelectedCameraName"]?.ToString())
                    cbCamera.SelectedIndex = i;

            foreach (var microphone in Devices.Instance.AudioCapturersList)
                MicrophonesList.Add(microphone.Name);

            cbMicrophone.ItemsSource = MicrophonesList;

            cbMicrophone.SelectionChanged += CbMicrophone_SelectionChanged;

            ItemCollection microphones = cbMicrophone.Items;

            for (int i = 0; i < microphones.Count; i++)
                if (microphones[i].ToString() == localSettings.Values["SelectedMicrophoneName"]?.ToString())
                    cbMicrophone.SelectedIndex = i;

            foreach (var speaker in Devices.Instance.AudioRendersList)
                SpeakersList.Add(speaker.Name);

            cbSpeakers.ItemsSource = SpeakersList;

            cbSpeakers.SelectionChanged += CbSpeakers_SelectionChanged;

            ItemCollection speakers = cbSpeakers.Items;

            for (int i = 0; i < speakers.Count; i++)
                if (speakers[i].ToString() == localSettings.Values["SelectedSpeakerName"]?.ToString())
                    cbSpeakers.SelectedIndex = i;

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

        private void CbSpeakers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localSettings.Values["SelectedSpeakerName"] = (string)cbSpeakers.SelectedValue;
        }

        private void CbMicrophone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localSettings.Values["SelectedMicrophoneName"] = (string)cbMicrophone.SelectedValue;
        }

        private void CbCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localSettings.Values["SelectedCameraName"] = (string)cbCamera.SelectedValue;
        }
    }
}
