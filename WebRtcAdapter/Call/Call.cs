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

        /// <summary>
        /// Send JSON string to remote peer.
        /// </summary>
        public event EventHandler<string> OnSendMessageToRemotePeer;

        public async Task<ICallInfo> PlaceCallAsync(CallConfiguration config)
        {
            //if (PeerConnection != null)
            //{
            //    Debug.WriteLine("[Error] We only support connection to one peer at a time.");
            //    return null;
            //}

            var offerOptions = new RTCOfferOptions();
            offerOptions.OfferToReceiveAudio = true;
            offerOptions.OfferToReceiveVideo = true;
            IRTCSessionDescription offer = await PeerConnection.CreateOffer(offerOptions);

            // Alter sdp to force usage of selected codecs
            string modifiedSdp = offer.Sdp;
            //SdpUtils.SelectCodecs(ref modifiedSdp, int.Parse(config.PreferredAudioCodecId), int.Parse(config.PreferredVideoCodecId));
            var sdpInit = new RTCSessionDescriptionInit();
            sdpInit.Sdp = modifiedSdp;
            sdpInit.Type = offer.SdpType;
            var modifiedOffer = new RTCSessionDescription(sdpInit);

            await PeerConnection.SetLocalDescription(modifiedOffer);

            Debug.WriteLine($"Sending offer: {modifiedOffer.Sdp}");

            string jsonString = SdpToJsonString(modifiedOffer);

            CallInfo callInfo = new CallInfo();
            callInfo.SetCall(new Call());
            callInfo.SetSdp(modifiedSdp);
            callInfo.SetJsonString(jsonString);

            OnSendMessageToRemotePeer.Invoke(this, jsonString);

            return callInfo;
        }

        // Public events to notify about connection status
        public event Action OnPeerConnectionCreated;

        /// <summary>
        /// Creates a peer connection.
        /// </summary>
        /// <returns>True if connection to a peer is successfully created.</returns>
        private bool CreatePeerConnection()
        {
            Debug.Assert(PeerConnection == null);

            Debug.WriteLine("Creating peer connection.");
            PeerConnection = new RTCPeerConnection(ConfigureRtc());

            OnPeerConnectionCreated?.Invoke();

            if (PeerConnection == null)
                throw new NullReferenceException("Peer connection is not created.");

            PeerConnection.OnIceGatheringStateChange += PeerConnection_OnIceGatheringStateChange;
            PeerConnection.OnIceConnectionStateChange += PeerConnection_OnIceConnectionStateChange;

            //PeerConnection.OnIceCandidate += PeerConnection_OnIceCandidate;
            //PeerConnection.OnTrack += PeerConnection_OnTrack;
            //PeerConnection.OnRemoveTrack += PeerConnection_OnRemoveTrack;

            //GetUserMedia();

            //AddLocalMediaTracks();

            //BindSelfVideo();

            return true;
        }

        private void PeerConnection_OnIceGatheringStateChange()
        {
            Debug.WriteLine("Ice connection state change, gathering-state = "
                    + PeerConnection.IceGatheringState.ToString().ToLower());
        }

        private void PeerConnection_OnIceConnectionStateChange()
        {
            Debug.WriteLine("Ice connection state change, state="
                    + (PeerConnection != null ? PeerConnection.IceConnectionState.ToString().ToLower() : "closed"));
        }

        private readonly List<RTCIceServer> _iceServers;
        private WebRtcFactory _factory;

        private RTCConfiguration ConfigureRtc()
        {
            var factoryConfig = new WebRtcFactoryConfiguration();
            _factory = new WebRtcFactory(factoryConfig);

            var config = new RTCConfiguration()
            {
                Factory = _factory,
                BundlePolicy = RTCBundlePolicy.Balanced,
                IceTransportPolicy = RTCIceTransportPolicy.All,
                IceServers = _iceServers
            };

            return config;
        }

        /// <summary>
        /// Creates JSON object from SDP.
        /// </summary>
        /// <param name="description">RTC session description.</param>
        /// <returns>JSON object.</returns>
        private string SdpToJsonString(IRTCSessionDescription description)
        {
            JsonObject json = null;
            Debug.WriteLine($"Sent session description: {description.Sdp}");

            string messageType = null;

            switch (description.SdpType)
            {
                case RTCSdpType.Offer: messageType = "offer"; break;
                case RTCSdpType.Answer: messageType = "answer"; break;
                case RTCSdpType.Pranswer: messageType = "pranswer"; break;
                default: Debug.Assert(false, description.SdpType.ToString()); break;
            }

            json = new JsonObject
            {
                { NegotiationAtributes.Type, JsonValue.CreateStringValue(messageType) },
                { NegotiationAtributes.Sdp, JsonValue.CreateStringValue(description.Sdp) }
            };

            return json.Stringify();

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
