using ClientCore.Call;
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

        private int _peerId = -1;

        public void MessageFromPeerTaskRun(int peerId, string content)
        {
            PeerId = peerId;

            Task.Run(async () =>
            {
                Debug.Assert(_peerId == PeerId || _peerId == -1);
                Debug.Assert(content.Length > 0);

                if (_peerId != PeerId && _peerId != -1)
                {
                    Debug.WriteLine("Received a message from unknown peer " +
                        "while already in a conversation with a different peer.");

                    return;
                }

                if (!JsonObject.TryParse(content, out JsonObject jMessage))
                {
                    Debug.WriteLine($"Received unknown message: {content}");
                    return;
                }

                string type = jMessage.ContainsKey(NegotiationAtributes.Type)
                       ? jMessage.GetNamedString(NegotiationAtributes.Type)
                       : null;

                if (PeerConnection == null)
                {
                    if (!string.IsNullOrEmpty(type))
                    {
                        // Create the peer connection only when call is 
                        // about to get initiated. Otherwise ignore the 
                        // message from peers which could be result 
                        // of old (but not yet fully closed) connections.
                        if (type == "offer" || type == "answer" || type == "json")
                        {
                            Debug.Assert(_peerId == -1);
                            _peerId = PeerId;

                            if (!CreatePeerConnection())
                            {
                                Debug.WriteLine("Failed to initialize our PeerConnection instance");

                                OnSignedOut.Invoke(this, null);
                                return;
                            }
                            else if (_peerId != PeerId)
                            {
                                Debug.WriteLine("Received a message from unknown peer while already " +
                                    "in a conversation with a different peer.");

                                return;
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("[Warn] Received an untyped message after closing peer connection.");
                        return;
                    }
                }

                if (PeerConnection != null && !string.IsNullOrEmpty(type))
                {
                    if (type == "offer-loopback")
                    {
                        // Loopback not supported
                        Debug.Assert(false);
                    }

                    string sdp = null;

                    sdp = jMessage.ContainsKey(NegotiationAtributes.Sdp)
                          ? jMessage.GetNamedString(NegotiationAtributes.Sdp)
                          : null;

                    if (string.IsNullOrEmpty(sdp))
                    {
                        Debug.WriteLine("[Error] Can't parse received session description message.");
                        return;
                    }

                    Debug.WriteLine($"Received session description:\n{content}");

                    RTCSdpType messageType = RTCSdpType.Offer;
                    switch (type)
                    {
                        case "offer": messageType = RTCSdpType.Offer; break;
                        case "answer": messageType = RTCSdpType.Answer; break;
                        case "pranswer": messageType = RTCSdpType.Pranswer; break;
                        default: Debug.Assert(false, type); break;
                    }

                    var sdpInit = new RTCSessionDescriptionInit();
                    sdpInit.Sdp = sdp;
                    sdpInit.Type = messageType;
                    var description = new RTCSessionDescription(sdpInit);

                    await PeerConnection.SetRemoteDescription(description);

                    if (messageType == RTCSdpType.Offer)
                    {
                        var answerOptions = new RTCAnswerOptions();
                        IRTCSessionDescription answer = await PeerConnection.CreateAnswer(answerOptions);
                        await PeerConnection.SetLocalDescription(answer);
                        string jsonString = SdpToJsonString(answer);
                        // Send answer
                        OnSendMessageToRemotePeer.Invoke(this, jsonString);
                    }
                }
                else
                {
                    RTCIceCandidate candidate = null;

                    string sdpMid = jMessage.ContainsKey(NegotiationAtributes.SdpMid)
                           ? jMessage.GetNamedString(NegotiationAtributes.SdpMid)
                           : null;

                    double sdpMLineIndex = jMessage.ContainsKey(NegotiationAtributes.SdpMLineIndex)
                           ? jMessage.GetNamedNumber(NegotiationAtributes.SdpMLineIndex)
                           : -1;

                    string sdpCandidate = jMessage.ContainsKey(NegotiationAtributes.Candidate)
                           ? jMessage.GetNamedString(NegotiationAtributes.Candidate)
                           : null;

                    if (string.IsNullOrEmpty(sdpMid) || sdpMLineIndex == -1 || string.IsNullOrEmpty(sdpCandidate))
                    {
                        Debug.WriteLine($"[Error] Can't parse received message.\n{content}");
                        return;
                    }

                    var candidateInit = new RTCIceCandidateInit();
                    candidateInit.Candidate = sdpCandidate;
                    candidateInit.SdpMid = sdpMid;
                    candidateInit.SdpMLineIndex = (ushort)sdpMLineIndex;
                    candidate = new RTCIceCandidate(candidateInit);

                    await PeerConnection.AddIceCandidate(candidate);

                    Debug.WriteLine($"Receiving ice candidate:\n{content}");
                }
            }).Wait();
        }

        /// <summary>
        /// Send JSON string to remote peer.
        /// </summary>
        public event EventHandler<string> OnSendMessageToRemotePeer;

        /// <summary>
        /// Sign out peer from the server
        /// </summary>
        public event EventHandler OnSignedOut;

        public async Task<ICallInfo> PlaceCallAsync(CallConfiguration config)
        {
            Debug.Assert(_peerId == -1);

            if (PeerConnection != null)
            {
                Debug.WriteLine("[Error] We only support connection to one peer at a time.");
                return null;
            }

            if (CreatePeerConnection())
            {
                _peerId = PeerId;

                var offerOptions = new RTCOfferOptions();
                offerOptions.OfferToReceiveAudio = true;
                offerOptions.OfferToReceiveVideo = true;
                IRTCSessionDescription offer = await PeerConnection.CreateOffer(offerOptions);

                // Alter sdp to force usage of selected codecs
                string modifiedSdp = offer.Sdp;
                //TODO: SdpUtils.SelectCodecs(ref modifiedSdp, int.Parse(config.PreferredAudioCodecId), int.Parse(config.PreferredVideoCodecId));
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
            return null;
        }

        // Public events to notify about connection status
        public event Action OnPeerConnectionCreated;
        public event Action OnPeerConnectionClosed;
        public event Action OnReadyToConnect;

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

            AddLocalMediaTracks();

            BindSelfVideo();

            return true;
        }

        private void BindSelfVideo()
        {
            if (_selfVideoTrack != null)
            {

                _selfVideoTrack.Element = MediaElementMaker.Bind(Devices.Instance.SelfVideo);
                ((MediaStreamTrack)_selfVideoTrack).OnFrameRateChanged += (float frameRate) =>
                {
                    FramesPerSecondChanged?.Invoke("SELF", frameRate.ToString("0.0"));
                };
                ((MediaStreamTrack)_selfVideoTrack).OnResolutionChanged += (uint width, uint height) =>
                {
                    ResolutionChanged?.Invoke("SELF", width, height);
                };
            }
        }

        public event Action<IMediaStreamTrack> OnAddLocalTrack;

        private void AddLocalMediaTracks()
        {
            Debug.WriteLine("Adding local media tracks.");
            PeerConnection.AddTrack(_selfVideoTrack);
            PeerConnection.AddTrack(_selfAudioTrack);

            OnAddLocalTrack?.Invoke(_selfVideoTrack);
            OnAddLocalTrack?.Invoke(_selfAudioTrack);
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
                    _peerVideoTrack.Element = MediaElementMaker.Bind(Devices.Instance.PeerVideo);
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

        private readonly List<RTCIceServer> _iceServers = new List<RTCIceServer>();
        private WebRtcFactory _factory;

        private RTCConfiguration ConfigureRtc()
        {
            AddIceServers(AddDefaultIceServers);

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

        public void AddIceServers(List<IceServer> iceServersList)
        {
            List<string> urlsList = new List<string>();

            foreach (IceServer ice in iceServersList)
            {
                foreach (string url in ice.Urls)
                {
                    string checkedUrl = string.Empty;

                    if (url.StartsWith("stun"))
                    {
                        checkedUrl = url;
                        if (!url.StartsWith("stun:"))
                        {
                            checkedUrl = $"stun:{url}";
                        }
                    }
                    if (url.StartsWith("turn"))
                    {
                        checkedUrl = url;
                        if (!url.StartsWith("turn:"))
                        {
                            checkedUrl = $"turn:{url}";
                        }
                    }
                    urlsList.Add(checkedUrl);
                }

                RTCIceServer server = new RTCIceServer { Urls = urlsList };

                if (ice.Username != null)
                    server.Username = ice.Username;
                if (ice.Credential != null)
                    server.Credential = ice.Credential;

                _iceServers.Add(server);
            }
        }

        public static List<IceServer> AddDefaultIceServers
            => new List<IceServer>()
            {
                new IceServer { Urls = new List<string> { "stun:stun.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun1.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun2.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun3.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun4.l.google.com:19302" } }
            };

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

        public int PeerId { get; set; }

        public object MediaLock { get; set; } = new object();

        /// <summary>
        /// Closes a peer connection.
        /// </summary>
        public void ClosePeerConnection()
        {
            lock (MediaLock)
            {
                if (PeerConnection != null)
                {
                    _peerId = -1;

                    PeerConnection.OnIceCandidate -= PeerConnection_OnIceCandidate;
                    PeerConnection.OnTrack -= PeerConnection_OnTrack;
                    PeerConnection.OnRemoveTrack -= PeerConnection_OnRemoveTrack;

                    if (_peerVideoTrack != null) _peerVideoTrack.Element = null;
                    if (_selfVideoTrack != null) _selfVideoTrack.Element = null;

                    (_peerVideoTrack as IDisposable)?.Dispose();
                    (_peerAudioTrack as IDisposable)?.Dispose();
                    (_selfVideoTrack as IDisposable)?.Dispose();
                    (_selfAudioTrack as IDisposable)?.Dispose();

                    _peerVideoTrack = null;
                    _peerAudioTrack = null;
                    _selfVideoTrack = null;
                    _selfAudioTrack = null;

                    OnPeerConnectionClosed?.Invoke();

                    PeerConnection = null;

                    OnReadyToConnect?.Invoke();

                    GC.Collect(); // Ensure all references are truly dropped.
                }
            }
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
