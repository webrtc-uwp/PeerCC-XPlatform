using Client_UWP.Models;
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

        private List<string> _camerasList = new List<string>();
        private List<string> _microphonesList = new List<string>();
        private List<string> _speakersList = new List<string>();
        private List<string> _audioCodesList = new List<string>();
        private List<string> _videoCodecsList = new List<string>();

        public SettingsDevicesPage()
        {
            InitializeComponent();

            InitView();
        }

        private void InitView()
        {
            GoToMainPage.Click += (sender, args) => Frame.Navigate(typeof(MainPage));

            AccountSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsAccountPage));

            ConnectionSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsConnectionPage));

            DebugSettings.Click += (sender, args) => Frame.Navigate(typeof(SettingsDebugPage));

            foreach (var videoDevice in Devices.Instance.VideoDevicesList)
                _camerasList.Add(videoDevice.Name);

            cbCamera.ItemsSource = _camerasList;

            cbCamera.SelectionChanged += CbCamera_SelectionChanged;

            ItemCollection cameras = cbCamera.Items;

            for (int i = 0; i < cameras.Count; i++)
                if (cameras[i].ToString() == localSettings.Values["SelectedCameraName"]?.ToString())
                    cbCamera.SelectedIndex = i;

            foreach (var microphone in Devices.Instance.AudioCapturersList)
                _microphonesList.Add(microphone.Name);

            cbMicrophone.ItemsSource = _microphonesList;

            cbMicrophone.SelectionChanged += CbMicrophone_SelectionChanged;

            ItemCollection microphones = cbMicrophone.Items;

            for (int i = 0; i < microphones.Count; i++)
                if (microphones[i].ToString() == localSettings.Values["SelectedMicrophoneName"]?.ToString())
                    cbMicrophone.SelectedIndex = i;

            foreach (var speaker in Devices.Instance.AudioRendersList)
                _speakersList.Add(speaker.Name);

            cbSpeakers.ItemsSource = _speakersList;

            cbSpeakers.SelectionChanged += CbSpeakers_SelectionChanged;

            ItemCollection speakers = cbSpeakers.Items;

            for (int i = 0; i < speakers.Count; i++)
                if (speakers[i].ToString() == localSettings.Values["SelectedSpeakerName"]?.ToString())
                    cbSpeakers.SelectedIndex = i;

            foreach (var audioCodec in DefaultSettings.GetAudioCodecs)
                _audioCodesList.Add(audioCodec.Name);

            cbAudioCodecs.ItemsSource = _audioCodesList;

            cbAudioCodecs.SelectionChanged += CbAudioCodec_SelectionChanged;

            ItemCollection audioCodecs = cbAudioCodecs.Items;

            for (int i = 0; i < audioCodecs.Count; i++)
                if (audioCodecs[i].ToString() == localSettings.Values["SelectedAudioCodec"]?.ToString())
                    cbAudioCodecs.SelectedIndex = i;

            foreach (var videoCodec in DefaultSettings.GetVideoCodecs)
                _videoCodecsList.Add(videoCodec.Name);

            cbVideoCodecs.ItemsSource = _videoCodecsList;

            cbVideoCodecs.SelectionChanged += CbVideoCodecs_SelectionChanged;

            ItemCollection videoCodecs = cbVideoCodecs.Items;

            for (int i = 0; i < videoCodecs.Count; i++)
                if (videoCodecs[i].ToString() == localSettings.Values["SelectedVideoCodec"]?.ToString())
                    cbVideoCodecs.SelectedIndex = i;

            //List<string> acList = new List<string>();

            //foreach (var ac in ViewModel.AudioCodecsList)
            //    acList.Add(ac.Name);

            //cbAudioCodecs.ItemsSource = acList;
            //cbAudioCodecs.SelectedIndex = 0;

            //List<string> vcList = new List<string>();

            //foreach (VideoCodec vc in ViewModel.VideoCodecsList)
            //    vcList.Add(vc.Name);

            //cbVideoCodecs.ItemsSource = vcList;
            //cbVideoCodecs.SelectedIndex = 0;
        }

        private void CbVideoCodecs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localSettings.Values["SelectedVideoCodec"] = (string)cbVideoCodecs.SelectedValue;
        }

        private void CbAudioCodec_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            localSettings.Values["SelectedAudioCodec"] = (string)cbAudioCodecs.SelectedValue;
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
