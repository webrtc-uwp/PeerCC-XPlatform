using ClientCore.Call;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace WebRtcAdapter.Call
{
    public class Call : ICall
    {
        public event FrameRateChangeHandler OnFrameRateChanged;
        public event ResolutionChangeHandler OnResolutionChanged;


        private readonly object _peerConnectionLock = new object();
        private RTCPeerConnection _peerConnection_DoNotUse;
        public RTCPeerConnection PeerConnection
        {
            get
            {
                lock (_peerConnectionLock)
                {
                    return _peerConnection_DoNotUse;
                }
            }
            set
            {
                lock (_peerConnectionLock)
                {
                    if (value == null)
                    {
                        if (_peerConnection_DoNotUse != null)
                        {
                            (_peerConnection_DoNotUse as IDisposable)?.Dispose();
                        }
                    }
                    _peerConnection_DoNotUse = value;
                }
            }
        }

        public async Task<ICallInfo> PlaceCallAsync(CallConfiguration config)
        {
            var offerOptions = new RTCOfferOptions();
            offerOptions.OfferToReceiveAudio = true;
            offerOptions.OfferToReceiveVideo = true;
            IRTCSessionDescription offer = await PeerConnection.CreateOffer(offerOptions);

            //if (localSettings.Values["SelectedAudioCodecName"] != null)
            //{
            //    foreach (var aCodec in Devices.Instance.AudioCodecsList)
            //    {
            //        if (aCodec.DisplayName == (string)localSettings.Values["SelectedAudioCodecName"])
            //            AudioCodec = (Codec)aCodec;
            //    }
            //}
            //else AudioCodec = (Codec)Devices.Instance.AudioCodecsList.First();

            //if (localSettings.Values["SelectedVideoCodecName"] != null)
            //{
            //    foreach (var vCodec in Devices.Instance.VideoCodecsList)
            //    {
            //        if (vCodec.DisplayName == (string)localSettings.Values["SelectedVideoCodecName"])
            //            VideoCodec = (Codec)vCodec;
            //    }
            //}
            //else
            //    VideoCodec = (Codec)Devices.Instance.VideoCodecsList.First();

            // Alter sdp to force usage of selected codecs
            string modifiedSdp = offer.Sdp;
            //SdpUtils.SelectCodecs(ref modifiedSdp, int.Parse(AudioCodec.Id), int.Parse(VideoCodec.Id));
            var sdpInit = new RTCSessionDescriptionInit();
            sdpInit.Sdp = modifiedSdp;
            sdpInit.Type = offer.SdpType;
            var modifiedOffer = new RTCSessionDescription(sdpInit);

            await PeerConnection.SetLocalDescription(modifiedOffer);

            Debug.WriteLine($"Sending offer: {modifiedOffer.Sdp}");

            JsonObject json = SdpToJson(modifiedOffer);

            CallInfo callInfo = new CallInfo();
            callInfo.SetCall(new Call());
            callInfo.SetSdp(modifiedSdp);
            callInfo.SetJson(json);

            return callInfo;
        }

        /// <summary>
        /// Creates JSON object from SDP.
        /// </summary>
        /// <param name="description">RTC session description.</param>
        /// <returns>JSON object.</returns>
        private JsonObject SdpToJson(IRTCSessionDescription description)
        {
            JsonObject json = null;
            Debug.WriteLine($"Sent session description: {description.Sdp}");

            json = new JsonObject();
            string messageType = null;

            switch (description.SdpType)
            {
                case RTCSdpType.Offer: messageType = "offer"; break;
                case RTCSdpType.Answer: messageType = "answer"; break;
                case RTCSdpType.Pranswer: messageType = "pranswer"; break;
                default: Debug.Assert(false, description.SdpType.ToString()); break;
            }

            return new JsonObject
            {
                { NegotiationAtributes.Type, JsonValue.CreateStringValue(messageType) },
                { NegotiationAtributes.Sdp, JsonValue.CreateStringValue(description.Sdp) }
            };

            //SendMessage(json);
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

        // SDP negotiation attributes
        public class NegotiationAtributes
        {
            public static readonly string SdpMid = "sdpMid";
            public static readonly string SdpMLineIndex = "sdpMLineIndex";
            public static readonly string Candidate = "candidate";
            public static readonly string Type = "type";
            public static readonly string Sdp = "sdp";
        }
    }
}
