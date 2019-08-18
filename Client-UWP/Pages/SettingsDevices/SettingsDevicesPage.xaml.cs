using Client_UWP.Pages.Main;
using Client_UWP.Pages.SettingsAccount;
using Client_UWP.Pages.SettingsConnection;
using Client_UWP.Pages.SettingsDebug;
using ClientCore.Call;
using System.Collections.ObjectModel;
using System.Linq;
using WebRtcAdapter;
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

        private static ObservableCollection<string> _camerasList;
        private static ObservableCollection<string> _microphonesList;
        private static ObservableCollection<string> _speakersList;
        private static ObservableCollection<string> _audioCodesList;
        private static ObservableCollection<string> _videoCodecsList;
        private static ObservableCollection<string> _frameRatesList;
        private static ObservableCollection<string> _resolutionsList;

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

            _camerasList = new ObservableCollection<string>();
            _microphonesList = new ObservableCollection<string>();
            _speakersList = new ObservableCollection<string>();
            _audioCodesList = new ObservableCollection<string>();
            _videoCodecsList = new ObservableCollection<string>();
            _frameRatesList = new ObservableCollection<string>();
            _resolutionsList = new ObservableCollection<string>();

            SetCamerasList();
            SetMicrophonesList();
            SetSpeakersList();
            SetAudioCodecsList();
            SetVideoCodecsList();

            cbCaptureFrameRate.SelectionChanged += CbCaptureFrameRate_SelectionChanged;
        }

        private void SetVideoCodecsList()
        {
            foreach (ICodec videoCodec in Devices.Instance.VideoCodecsList)
                _videoCodecsList.Add(videoCodec.DisplayName);

            if (_videoCodecsList.Count != 0)
            {
                _localSettings.SerializeVideoCodecsNameList(null);
                _localSettings.SerializeVideoCodecsNameList(_videoCodecsList);
            }
            else
                _videoCodecsList = _localSettings.DeserializeVideoCodecsNameList();

            cbVideoCodecs.SelectionChanged += CbVideoCodecs_SelectionChanged;

            if (_videoCodecsList != null)
            {
                cbVideoCodecs.ItemsSource = _videoCodecsList;

                if (_localSettings.GetSelectedVideoCodecName != null)
                {
                    for (int i = 0; i < _videoCodecsList.Count; i++)
                        if (_videoCodecsList[i] == (string)_localSettings.GetSelectedVideoCodecName)
                            cbVideoCodecs.SelectedIndex = i;
                }
                else
                    cbVideoCodecs.SelectedIndex = -1;
            }
        }

        private void SetAudioCodecsList()
        {
            foreach (ICodec audioCodec in Devices.Instance.AudioCodecsList)
                _audioCodesList.Add(audioCodec.DisplayName);

            if (_audioCodesList.Count != 0)
            {
                _localSettings.SerializeVideoCodecsNameList(null);
                _localSettings.SerializeVideoCodecsNameList(_audioCodesList);
            }
            else
                _audioCodesList = _localSettings.DeserializeAudioCodecsNameList();

            cbAudioCodecs.SelectionChanged += CbAudioCodec_SelectionChanged;

            if (_audioCodesList != null)
            {
                cbAudioCodecs.ItemsSource = _audioCodesList;

                if (_localSettings.GetSelectedAudioCodecName != null)
                {
                    for (int i = 0; i < _audioCodesList.Count; i++)
                        if (_audioCodesList[i] == (string)_localSettings.GetSelectedAudioCodecName)
                            cbAudioCodecs.SelectedIndex = i;
                }
                else
                    cbAudioCodecs.SelectedIndex = -1;
            }
        }

        private void SetSpeakersList()
        {
            foreach (IMediaDevice speaker in Devices.Instance.AudioMediaDevicesRendersList)
                _speakersList.Add(speaker.DisplayName);

            cbSpeakers.SelectionChanged += CbSpeakers_SelectionChanged;

            cbSpeakers.ItemsSource = _speakersList;

            if (_localSettings.GetSelectedSpeakerName != null)
            {
                for (int i = 0; i < _speakersList.Count; i++)
                    if (_speakersList[i] == (string)_localSettings.GetSelectedSpeakerName)
                        cbSpeakers.SelectedIndex = i;
            }
            else
                cbSpeakers.SelectedIndex = -1;
        }

        private void SetMicrophonesList()
        {
            foreach (IMediaDevice microphone in Devices.Instance.AudioMediaDevicesCapturersList)
                _microphonesList.Add(microphone.DisplayName);

            cbMicrophone.SelectionChanged += CbMicrophone_SelectionChanged;

            cbMicrophone.ItemsSource = _microphonesList;

            if (_localSettings.GetSelectedMicrophoneName != null)
            {
                for (int i = 0; i < _microphonesList.Count; i++)
                    if (_microphonesList[i] == (string)_localSettings.GetSelectedMicrophoneName)
                        cbMicrophone.SelectedIndex = i;
            }
            else
                cbMicrophone.SelectedIndex = -1;
        }

        private void SetCamerasList()
        {
            foreach (IMediaDevice videoDevice in Devices.Instance.VideoMediaDevicesList)
                _camerasList.Add(videoDevice.DisplayName);

            cbCamera.SelectionChanged += CbCamera_SelectionChanged;

            cbCamera.ItemsSource = _camerasList;

            if (_localSettings.GetSelectedCameraName != null)
            {
                for (int i = 0; i < _camerasList.Count; i++)
                    if (_camerasList[i] == (string)_localSettings.GetSelectedCameraName)
                        cbCamera.SelectedIndex = i;
            }
            else
                cbCamera.SelectedIndex = -1;
        }

        private void SetResolutions()
        {
            foreach (IMediaDevice device in Devices.Instance.VideoMediaDevicesList)
            {
                if (device.DisplayName == (string)_localSettings.GetSelectedCameraName)
                {
                    _resolutionsList.Clear();
                    cbCaptureResolution.SelectedIndex = -1;
                    foreach (var resolution in device.VideoFormats.OrderBy(v => v.Dimension.Width))
                    {
                        _resolutionsList.Add(resolution.Dimension.Width.ToString() + " x " + resolution.Dimension.Height.ToString());
                    }
                }
            }

            cbCaptureResolution.SelectionChanged += CbCaptureResolution_SelectionChanged;

            cbCaptureResolution.ItemsSource = _resolutionsList;

            if (_localSettings.GetSelectedResolutionString != null)
            {
                for (int i = 0; i < _resolutionsList.Count; i++)
                    if (_resolutionsList[i] == (string)_localSettings.GetSelectedResolutionString)
                        cbCaptureResolution.SelectedIndex = i;
            }
            else
                cbCaptureResolution.SelectedIndex = -1;
        }

        private void SetFrameRateList()
        {
            foreach (IMediaDevice device in Devices.Instance.VideoMediaDevicesList)
            {
                if (device.DisplayName == (string)_localSettings.GetSelectedCameraName)
                {
                    foreach (var resolution in device.VideoFormats)
                    {
                        if ((resolution.Dimension.Width.ToString() + " x " + resolution.Dimension.Height.ToString()) == (string)_localSettings.GetSelectedResolutionString)
                        {
                            //GuiLogic.Instance.Call_OnResolutionChanged(MediaDirection.Local, resolution.Dimension);

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

            cbCaptureFrameRate.ItemsSource = _frameRatesList;

            if (_localSettings.GetSelectedFrameRateString != null)
            {
                for (int i = 0; i < _frameRatesList.Count; i++)
                    if (_frameRatesList[i] == (string)_localSettings.GetSelectedFrameRateString)
                        cbCaptureFrameRate.SelectedIndex = i;
            }
            else
                cbCaptureFrameRate.SelectedIndex = -1;
        }

        private void CbCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _localSettings.SetSelectedCameraName((string)cbCamera.SelectedValue);

            SetResolutions();
        }


        private void CbCaptureResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _localSettings.SetSelectedResolutionString((string)cbCaptureResolution.SelectedValue);

            if (cbCaptureResolution.SelectedValue != null)
                SetFrameRateList();
        }

        private void CbCaptureFrameRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _localSettings.SetSelectedFrameRateString((string)cbCaptureFrameRate.SelectedValue);

            //if (cbCaptureFrameRate.SelectedValue != null)
            //    GuiLogic.Instance.Call_OnFrameRateChanged(MediaDirection.Local, int.Parse((string)_localSettings.GetSelectedFrameRateString));
        }

        private void CbVideoCodecs_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _localSettings.SetSelectedVideoCodecName((string)cbVideoCodecs.SelectedValue);

        private void CbAudioCodec_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _localSettings.SetSelectedAudioCodecName((string)cbAudioCodecs.SelectedValue);

        private void CbSpeakers_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _localSettings.SetSelectedSpeakerName((string)cbSpeakers.SelectedValue);

        private void CbMicrophone_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            _localSettings.SetSelectedMicrophoneName((string)cbMicrophone.SelectedValue);
    }
}
