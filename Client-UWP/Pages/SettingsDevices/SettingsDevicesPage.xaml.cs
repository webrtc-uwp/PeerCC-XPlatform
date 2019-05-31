using Client_UWP.Pages.Main;
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
        private static List<string> _frameRatesList;
        private static List<string> _resolutionsList;

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
            _frameRatesList = new List<string>();
            _resolutionsList = new List<string>();

            SetCamerasList();
            SetMicrophonesList();
            SetSpeakersList();
            SetAudioCodecsList();
            SetVideoCodecsList();
            SetResolutions();
        }

        private void SetVideoCodecsList()
        {
            _videoCodecsList = GuiLogic.Instance.VideoCodecsList;

            if (_videoCodecsList.Count != 0)
                _localSettings.SerializeVideoCodecsNameList(_videoCodecsList);

            cbVideoCodecs.ItemsSource = _localSettings.DeserializeVideoCodecsNameList();

            cbVideoCodecs.SelectionChanged += CbVideoCodecs_SelectionChanged;

            ItemCollection videoCodecs = cbVideoCodecs.Items;

            if (_localSettings.GetSelectedVideoCodecName != null)
            {
                for (int i = 0; i < videoCodecs.Count; i++)
                    if (videoCodecs[i].ToString() == (string)_localSettings.GetSelectedVideoCodecName)
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
                    if (audioCodecs[i].ToString() == (string)_localSettings.GetSelectedAudioCodecName)
                        cbAudioCodecs.SelectedIndex = i;
            }
            else
                cbAudioCodecs.SelectedIndex = 0;
        }

        private void SetSpeakersList()
        {
            foreach (var speaker in Devices.Instance.AudioMediaDevicesRendersList)
                _speakersList.Add(speaker.DisplayName);

            cbSpeakers.ItemsSource = _speakersList;

            cbSpeakers.SelectionChanged += CbSpeakers_SelectionChanged;

            ItemCollection speakers = cbSpeakers.Items;

            if (_localSettings.GetSelectedSpeakerName != null)
            {
                for (int i = 0; i < speakers.Count; i++)
                    if (speakers[i].ToString() == (string)_localSettings.GetSelectedSpeakerName)
                        cbSpeakers.SelectedIndex = i;
            }
            else
                cbSpeakers.SelectedIndex = 0;
        }

        private void SetMicrophonesList()
        {
            foreach (var microphone in Devices.Instance.AudioMediaDevicesCapturersList)
                _microphonesList.Add(microphone.DisplayName);

            cbMicrophone.ItemsSource = _microphonesList;

            cbMicrophone.SelectionChanged += CbMicrophone_SelectionChanged;

            ItemCollection microphones = cbMicrophone.Items;

            if (_localSettings.GetSelectedMicrophoneName != null)
            {
                for (int i = 0; i < microphones.Count; i++)
                    if (microphones[i].ToString() == (string)_localSettings.GetSelectedMicrophoneName)
                        cbMicrophone.SelectedIndex = i;
            }
            else
                cbMicrophone.SelectedIndex = 0;
        }

        private void SetCamerasList()
        {
            foreach (var videoDevice in Devices.Instance.VideoMediaDevicesList)
                _camerasList.Add(videoDevice.DisplayName);

            cbCamera.ItemsSource = _camerasList;

            cbCamera.SelectionChanged += CbCamera_SelectionChanged;

            ItemCollection cameras = cbCamera.Items;

            if (_localSettings.GetSelectedCameraName != null)
            {
                for (int i = 0; i < cameras.Count; i++)
                    if (cameras[i].ToString() == (string)_localSettings.GetSelectedCameraName)
                        cbCamera.SelectedIndex = i;
            }
            else
                cbCamera.SelectedIndex = 0;
        }

        private void SetResolutions()
        {
            foreach (var device in Devices.Instance.VideoMediaDevicesList)
            {
                if (device.DisplayName == (string)_localSettings.GetSelectedCameraName)
                {
                    foreach (var resolution in device.VideoFormats)
                    {
                        _resolutionsList.Add(resolution.Dimension.Width.ToString() + " x " + resolution.Dimension.Height.ToString());
                    }
                }
            }

            cbCaptureResolution.ItemsSource = _resolutionsList;

            cbCaptureResolution.SelectionChanged += CbCaptureResolution_SelectionChanged;

            ItemCollection resolutions = cbCaptureResolution.Items;

            if (_localSettings.GetSelectedResolutionString != null)
            {
                for (int i = 0; i < resolutions.Count; i++)
                    if ((string)resolutions[i] == (string)_localSettings.GetSelectedResolutionString)
                        cbCaptureResolution.SelectedIndex = i;
            }
            else
                cbCaptureResolution.SelectedIndex = 0;
        }

        private void SetFrameRateList()
        {
            foreach (var device in Devices.Instance.VideoMediaDevicesList)
            {
                if (device.DisplayName == (string)_localSettings.GetSelectedCameraName)
                {
                    foreach (var resolution in device.VideoFormats)
                    {
                        if ((resolution.Dimension.Width.ToString() + " x " + resolution.Dimension.Height.ToString()) == (string)_localSettings.GetSelectedResolutionString)
                        {
                            _frameRatesList.Clear();
                            cbCaptureFrameRate.SelectedIndex = -1;
                            foreach (var frameRate in resolution.FrameRates)
                            {
                                _frameRatesList.Add(frameRate.ToString());
                            }
                        }
                    }
                }
            }

            cbCaptureFrameRate.SelectionChanged += CbCaptureFrameRate_SelectionChanged;

            cbCaptureFrameRate.ItemsSource = _frameRatesList;

            if (_localSettings.GetSelectedFrameRateString != null)
            {
                for (int i = 0; i < _frameRatesList.Count; i++)
                    if (_frameRatesList[i].ToString() == (string)_localSettings.GetSelectedFrameRateString)
                        cbCaptureFrameRate.SelectedIndex = i;
            }
            else
                cbCaptureFrameRate.SelectedIndex = -1;
        }

        private void CbCaptureResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _localSettings.SetSelectedResolutionString((string)cbCaptureResolution.SelectedValue);

            SetFrameRateList();
        }

        private void CbCaptureFrameRate_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _localSettings.SetSelectedFrameRateString((string)cbCaptureFrameRate.SelectedValue);

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
