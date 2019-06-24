﻿using ClientCore.Call;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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

            PeerConnection.OnIceCandidate += PeerConnection_OnIceCandidate;
            PeerConnection.OnTrack += PeerConnection_OnTrack;
            PeerConnection.OnRemoveTrack += PeerConnection_OnRemoveTrack;

            GetUserMedia();

            //AddLocalMediaTracks();

            //BindSelfVideo();

            return true;
        }

        private void GetUserMedia()
        {
            Debug.WriteLine("Getting user media.");

            MediaDevice _selectedVideoDevice = (MediaDevice)Devices.Instance.VideoMediaDevicesList[0];

            for (int i = 0; i < Devices.Instance.VideoMediaDevicesList.Count; i++)
                if (Devices.Instance.VideoMediaDevicesList[i].DisplayName == (string)localSettings.Values["SelectedCameraName"])
                    _selectedVideoDevice = (MediaDevice)Devices.Instance.VideoMediaDevicesList[i];

            List<int> widths = new List<int>();
            List<int> heights = new List<int>();
            List<int> frameRates = new List<int>();

            foreach (var videoFormat in _selectedVideoDevice.VideoFormats)
            {
                widths.Add(videoFormat.Dimension.Width);
                heights.Add(videoFormat.Dimension.Height);

                foreach (var frameRate in videoFormat.FrameRates)
                    frameRates.Add(frameRate);
            }

            // Maximum and minimum values for the selected camera
            IReadOnlyList<IConstraint> mandatoryConstraints = new List<IConstraint>()
            {
                new Constraint("maxWidth", widths.Max().ToString()),
                new Constraint("minWidth", widths.Min().ToString()),
                new Constraint("maxHeight", heights.Max().ToString()),
                new Constraint("minHeight", heights.Min().ToString()),
                new Constraint("maxFrameRate", frameRates.Max().ToString()),
                new Constraint("minFrameRate", frameRates.Min().ToString())
            };

            // Add optional constrains
            IReadOnlyList<IConstraint> optionalConstraints = new List<IConstraint>();

            IMediaConstraints mediaConstraints = new MediaConstraints(mandatoryConstraints, optionalConstraints);

            var videoCapturer = VideoCapturer.Create(_selectedVideoDevice.DisplayName, _selectedVideoDevice.Id, false);

            var videoOptions = new VideoOptions();
            videoOptions.Factory = _factory;
            videoOptions.Capturer = videoCapturer;
            videoOptions.Constraints = mediaConstraints;

            var videoTrackSource = VideoTrackSource.Create(videoOptions);
            _selfVideoTrack = MediaStreamTrack.CreateVideoTrack("SELF_VIDEO", videoTrackSource);

            var audioOptions = new AudioOptions();
            audioOptions.Factory = _factory;

            var audioTrackSource = AudioTrackSource.Create(audioOptions);
            _selfAudioTrack = MediaStreamTrack.CreateAudioTrack("SELF_AUDIO", audioTrackSource);
        }

        /// <summary>
        /// Called when WebRTC detects another ICE candidate.
        /// This candidate needs to be sent to the other peer.
        /// </summary>
        /// <param name="Event">Details about RTCPeerConnectionIceEvent</param>
        private void PeerConnection_OnIceCandidate(IRTCPeerConnectionIceEvent Event)
        {
            if (Event.Candidate == null) return;

            double index = (double)Event.Candidate.SdpMLineIndex;

            JsonObject json = null;

            json = new JsonObject
            {
                { NegotiationAtributes.SdpMid, JsonValue.CreateStringValue(Event.Candidate.SdpMid) },
                { NegotiationAtributes.SdpMLineIndex, JsonValue.CreateNumberValue(index) },
                { NegotiationAtributes.Candidate, JsonValue.CreateStringValue(Event.Candidate.Candidate) }
            };

            Debug.WriteLine($"Send ice candidate:\n{json.Stringify()}");

            OnSendMessageToRemotePeer.Invoke(this, json.Stringify());
        }

        private IMediaStreamTrack _peerVideoTrack;
        private IMediaStreamTrack _selfVideoTrack;
        private IMediaStreamTrack _peerAudioTrack;
        private IMediaStreamTrack _selfAudioTrack;

        public Windows.UI.Xaml.Controls.MediaElement SelfVideo { get; set; }
        public Windows.UI.Xaml.Controls.MediaElement PeerVideo { get; set; }

        public event Action<string, string> FramesPerSecondChanged;
        public event Action<string, uint, uint> ResolutionChanged;

        /// <summary>
        /// Invoked when the remote peer added media stream to the peer connection.
        /// </summary>
        public event Action<IMediaStreamTrack> OnAddRemoteTrack;

        private void PeerConnection_OnTrack(IRTCTrackEvent Event)
        {
            if (Event.Track.Kind == "video")
            {
                _peerVideoTrack = Event.Track;

                if (_peerVideoTrack != null)
                {
                    _peerVideoTrack.Element = MediaElementMaker.Bind(PeerVideo);
                    ((MediaStreamTrack)_peerVideoTrack).OnFrameRateChanged += (float frameRate) =>
                    {
                        FramesPerSecondChanged?.Invoke("PEER", frameRate.ToString("0.0"));
                    };
                    ((MediaStreamTrack)_peerVideoTrack).OnResolutionChanged += (uint width, uint height) =>
                    {
                        ResolutionChanged?.Invoke("PEER", width, height);
                    };
                }
            }
            else if (Event.Track.Kind == "audio")
            {
                _peerAudioTrack = Event.Track;
            }

            OnAddRemoteTrack?.Invoke(Event.Track);
        }

        /// <summary>
        /// Invoked when the remote peer removed a media stream from the peer connection.
        /// </summary>
        public event Action<IMediaStreamTrack> OnRemoveRemoteTrack;

        private void PeerConnection_OnRemoveTrack(IRTCTrackEvent Event)
        {
            if (Event.Track.Kind == "video")
            {
                _peerVideoTrack.Element = null;
            }

            OnRemoveRemoteTrack?.Invoke(Event.Track);
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
