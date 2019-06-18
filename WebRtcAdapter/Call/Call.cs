using ClientCore.Call;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Windows.Storage;

namespace WebRtcAdapter.Call
{
    public class Call : ICall
    {
        public event FrameRateChangeHandler OnFrameRateChanged;
        public event ResolutionChangeHandler OnResolutionChanged;

        public Task<ICallInfo> PlaceCallAsync(CallConfiguration config)
        {
            return Task.Run(() => (ICallInfo)new CallInfo());
        }

        public Task<ICallInfo> AnswerCallAsync(CallConfiguration config, string sdpOfRemoteParty)
        {
            return Task.Run(() => (ICallInfo)new CallInfo());
        }

        public Task HangupAsync()
        {
            return Task.Run(() => Debug.WriteLine("Hangup async."));
        }

        private ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public string GetPreferredVideoFormatId(IList<IMediaDevice> VideoMediaDevicesList)
        {
            string selectedResolution = (string)localSettings.Values["SelectedResolution"];
            string preferredVideoFormatId = string.Empty;

            for (int i = 0; i < VideoMediaDevicesList.Count; i++)
            {
                for (int j = 0; j < VideoMediaDevicesList[i].VideoFormats.Count; j++)
                {
                    Size dimension = VideoMediaDevicesList[i].VideoFormats[j].Dimension;
                    string resolutionString = dimension.Width.ToString() + " x " + dimension.Height.ToString();
                    if (selectedResolution == resolutionString)
                        preferredVideoFormatId = VideoMediaDevicesList[i].VideoFormats[j].Id;
                }
            }

            return preferredVideoFormatId;
        }

        public int? GetPreferredFrameRate()
        {
            if (localSettings.Values["SelectedFrameRate"] != null)
                return int.Parse((string)localSettings.Values["SelectedFrameRate"]);
            else
                return -1;
        }

        public string GetPreferredVideoDeviceId(IList<IMediaDevice> VideoMediaDevicesList)
        {
            string selectedCameraName = (string)localSettings.Values["SelectedCameraName"];
            string preferredVideoDeviceId = string.Empty;

            for (int i = 0; i < VideoMediaDevicesList.Count; i++)
            {
                if (selectedCameraName == VideoMediaDevicesList[i].DisplayName)
                    preferredVideoDeviceId = VideoMediaDevicesList[i].Id;
            }

            return preferredVideoDeviceId;
        }

        public string GetPreferredOutputAudioDeviceId(IList<IMediaDevice> AudioMediaDevicesRendersList)
        {
            string selectedSpeakerName = (string)localSettings.Values["SelectedSpeakerName"];
            string preferredOutputAudioDeviceId = string.Empty;

            for (int i = 0; i < AudioMediaDevicesRendersList.Count; i++)
            {
                if (selectedSpeakerName == AudioMediaDevicesRendersList[i].DisplayName)
                    preferredOutputAudioDeviceId = AudioMediaDevicesRendersList[i].Id;
            }

            return preferredOutputAudioDeviceId;
        }

        public string GetPreferredInputAudioDeviceId(IList<IMediaDevice> AudioMediaDevicesCapturersList)
        {
            string selectedMicrophoneName = (string)localSettings.Values["SelectedMicrophoneName"];
            string preferredInputAudioDeviceId = string.Empty;

            for (int i = 0; i < AudioMediaDevicesCapturersList.Count; i++)
            {
                if (selectedMicrophoneName == AudioMediaDevicesCapturersList[i].DisplayName)
                    preferredInputAudioDeviceId = AudioMediaDevicesCapturersList[i].Id;
            }

            return preferredInputAudioDeviceId;
        }

        public string GetPreferredVideoCodecId(IList<ICodec> VideoCodecsList)
        {
            string selectedVideoCodecName = (string)localSettings.Values["SelectedVideoCodecName"];
            string preferredVideoCodecId = string.Empty;

            for (int i = 0; i < VideoCodecsList.Count; i++)
            {
                if (selectedVideoCodecName == VideoCodecsList[i].DisplayName)
                    preferredVideoCodecId = VideoCodecsList[i].Id;
            }

            return preferredVideoCodecId;
        }

        public string GetPreferredAudioCodecId(IList<ICodec> AudioCodecsList)
        {
            string selectedAudioCodecName = (string)localSettings.Values["SelectedAudioCodecName"];
            string preferredAudioCodecId = string.Empty;

            for (int i = 0; i < AudioCodecsList.Count; i++)
            {
                if (selectedAudioCodecName == AudioCodecsList[i].DisplayName)
                    preferredAudioCodecId = AudioCodecsList[i].Id;
            }

            return preferredAudioCodecId;
        }
    }
}
