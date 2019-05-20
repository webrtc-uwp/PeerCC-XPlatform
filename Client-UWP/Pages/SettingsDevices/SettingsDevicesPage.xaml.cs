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
        private LocalSettings _localSettings = new LocalSettings();

        private static List<string> _camerasList;
        private static List<string> _microphonesList;
        private static List<string> _speakersList;
        private static List<string> _audioCodesList;
        private static List<string> _videoCodecsList;

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

            _camerasList = new List<string>();
            _microphonesList = new List<string>();
            _speakersList = new List<string>();
            _audioCodesList = new List<string>();
            _videoCodecsList = new List<string>();

            SetCamerasList();
            SetMicrophonesList();
            SetSpeakersList();
            SetAudioCodecsList();
            SetVideoCodecsList();
        }

        private void SetVideoCodecsList()
        {
            foreach (var videoCodec in DefaultSettings.GetVideoCodecs)
                _videoCodecsList.Add(videoCodec.Name);

            cbVideoCodecs.ItemsSource = _videoCodecsList;

            cbVideoCodecs.SelectionChanged += CbVideoCodecs_SelectionChanged;

            ItemCollection videoCodecs = cbVideoCodecs.Items;

            if (_localSettings.GetSelectedVideoCodecName != null)
            {
                for (int i = 0; i < videoCodecs.Count; i++)
                    if (videoCodecs[i].ToString() == _localSettings.GetSelectedVideoCodecName.ToString())
                        cbVideoCodecs.SelectedIndex = i;
            }
            else
                cbVideoCodecs.SelectedIndex = 0;
        }

        private void SetAudioCodecsList()
        {
            foreach (var audioCodec in DefaultSettings.GetAudioCodecs)
                _audioCodesList.Add(audioCodec.Name);

            cbAudioCodecs.ItemsSource = _audioCodesList;

            cbAudioCodecs.SelectionChanged += CbAudioCodec_SelectionChanged;

            ItemCollection audioCodecs = cbAudioCodecs.Items;

            if (_localSettings.GetSelectedAudioCodecName != null)
            {
                for (int i = 0; i < audioCodecs.Count; i++)
                    if (audioCodecs[i].ToString() == _localSettings.GetSelectedAudioCodecName.ToString())
                        cbAudioCodecs.SelectedIndex = i;
            }
            else
                cbAudioCodecs.SelectedIndex = 0;
        }

        private void SetSpeakersList()
        {
            foreach (var speaker in Devices.Instance.AudioRendersList)
                _speakersList.Add(speaker.Name);

            cbSpeakers.ItemsSource = _speakersList;

            cbSpeakers.SelectionChanged += CbSpeakers_SelectionChanged;

            ItemCollection speakers = cbSpeakers.Items;

            if (_localSettings.GetSelectedSpeakerName != null)
            {
                for (int i = 0; i < speakers.Count; i++)
                    if (speakers[i].ToString() == _localSettings.GetSelectedSpeakerName.ToString())
                        cbSpeakers.SelectedIndex = i;
            }
            else
                cbSpeakers.SelectedIndex = 0;
        }

        private void SetMicrophonesList()
        {
            foreach (var microphone in Devices.Instance.AudioCapturersList)
                _microphonesList.Add(microphone.Name);

            cbMicrophone.ItemsSource = _microphonesList;

            cbMicrophone.SelectionChanged += CbMicrophone_SelectionChanged;

            ItemCollection microphones = cbMicrophone.Items;

            if (_localSettings.GetSelectedMicrophoneName != null)
            {
                for (int i = 0; i < microphones.Count; i++)
                    if (microphones[i].ToString() == _localSettings.GetSelectedMicrophoneName.ToString())
                        cbMicrophone.SelectedIndex = i;
            }
            else
                cbMicrophone.SelectedIndex = 0;
        }

        private void SetCamerasList()
        {
            foreach (var videoDevice in Devices.Instance.VideoDevicesList)
                _camerasList.Add(videoDevice.Name);

            cbCamera.ItemsSource = _camerasList;

            cbCamera.SelectionChanged += CbCamera_SelectionChanged;

            ItemCollection cameras = cbCamera.Items;

            if (_localSettings.GetSelectedCameraName != null)
            {
                for (int i = 0; i < cameras.Count; i++)
                    if (cameras[i].ToString() == _localSettings.GetSelectedCameraName.ToString())
                        cbCamera.SelectedIndex = i;
            }
            else
                cbCamera.SelectedIndex = 0;
        }

        private void CbVideoCodecs_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _localSettings.SetSelectedVideoCodecName((string)cbVideoCodecs.SelectedValue);

        private void CbAudioCodec_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _localSettings.SetSelectedAudioCodecName((string)cbAudioCodecs.SelectedValue);

        private void CbSpeakers_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _localSettings.SetSelectedSpeakerName((string)cbSpeakers.SelectedValue);

        private void CbMicrophone_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _localSettings.SetSelectedMicrophoneName((string)cbMicrophone.SelectedValue);

        private void CbCamera_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _localSettings.SetSelectedCameraName((string)cbCamera.SelectedValue);
    }
}
