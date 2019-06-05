using ClientCore.Account;
using ClientCore.Call;
using ClientCore.Signaling;
using GuiCore.Utilities;
using Org.WebRtc;
using PeerCC.Account;
using PeerCC.Signaling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebRtcAdapter.Call;
using Windows.Data.Json;
using Windows.Storage;

namespace GuiCore
{
    public class GuiLogic
    {
        private static GuiLogic instance = null;
        private static readonly object InstanceLock = new object();

        public static GuiLogic Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new GuiLogic();

                    return instance;
                }
            }
        }

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

        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public bool PeerConnectedToServer;

        public readonly HttpSignaler HttpSignaler;
        private readonly List<RTCIceServer> _iceServers;

        private WebRtcFactory _factory;

        // Public events to notify about connection status
        public event Action OnPeerConnectionCreated;
        public event Action OnPeerConnectionClosed;
        public event Action OnReadyToConnect;

        public Windows.UI.Xaml.Controls.MediaElement SelfVideo { get; set; }
        public Windows.UI.Xaml.Controls.MediaElement PeerVideo { get; set; }

        public Account Account;
        public Call Call;
        public Media Media;

        /// <summary>
        /// Video codec used in WebRTC session.
        /// </summary>
        public CodecInfoModel VideoCodec { get; set; }

        /// <summary>
        /// Audio codec used in WebRTC session.
        /// </summary>
        public CodecInfoModel AudioCodec { get; set; }

        private GuiLogic()
        {
            HttpSignaler = new HttpSignaler();
            _iceServers = new List<RTCIceServer>();
            PeerConnectedToServer = false;

            HttpSignaler.MessageFromPeer += HttpSignaler_MessageFromPeer;
        }

        private void HttpSignaler_MessageFromPeer(object sender, HttpSignalerMessageEvent e)
            => Instance.MessageFromPeerTaskRun(e.Message);

        public void SetAccount(string serviceUri)
        {
            IAccountProvider accountFactory =
                ClientCore.Factory.SignalingFactory.Singleton.CreateIAccountProvider();

            AccountProvider accountProvider = (AccountProvider)accountFactory;

            Account = (Account)accountProvider
                .GetAccount(serviceUri, HttpSignaler.LocalPeer.Name, HttpSignaler);
        }

        public void SetCall()
        {
            // Call
            ICallProvider callFactory =
                ClientCore.Factory.CallFactory.Singleton.CreateICallProvider();

            CallProvider callProvider = (CallProvider)callFactory;

            CallConfiguration callConfiguration = new CallConfiguration();
            callConfiguration.IceServers = iceServers;
            callConfiguration.PreferredInputAudioDeviceId = GetPreferredInputAudioDeviceId();
            callConfiguration.PreferredAudioOutputDeviceId = GetPreferredOutputAudioDeviceId();
            callConfiguration.PreferredVideoDeviceId = GetPreferredVideoDeviceId();

            callConfiguration.PreferredFrameRate = GetPreferredFrameRate();
            callConfiguration.PreferredVideoFormatId = null;

            callConfiguration.PreferredAudioCodecId = GetPreferredAudioCodecId();
            callConfiguration.PreferredVideoCodecId = GetPreferredVideoCodecId();

            callConfiguration.LocalVideoElement = null;
            callConfiguration.RemoteVideoElement = null;

            callProvider.PlaceCallAsync(callConfiguration);

            Call = (Call)callProvider.GetCall();

            Call.OnFrameRateChanged += (x, y) => { };
            Call.OnResolutionChanged += (x, y) => { };

            //Call.HangupAsync();
        }
        private List<IceServer> iceServers = new List<IceServer>();
        public void GetIceServers(List<IceServer> iceServersList)
        {
            iceServers = iceServersList;
        }

        private int? GetPreferredFrameRate()
        {
            string selectedFrameRate = (string)localSettings.Values["SelectedFrameRate"];
            string preferredFrameRate = selectedFrameRate.Substring(0, selectedFrameRate.Length - 3);

            return int.Parse(preferredFrameRate);
        }

        private string GetPreferredVideoDeviceId()
        {
            string selectedCameraName = (string)localSettings.Values["SelectedCameraName"];
            string preferredVideoDeviceId = string.Empty;

            for (int i = 0; i < Devices.Instance.VideoMediaDevicesList.Count; i++)
            {
                if (selectedCameraName == Devices.Instance.VideoMediaDevicesList[i].DisplayName)
                    preferredVideoDeviceId = Devices.Instance.VideoMediaDevicesList[i].Id;
            }

            return preferredVideoDeviceId;
        }

        private string GetPreferredOutputAudioDeviceId()
        {
            string selectedSpeakerName = (string)localSettings.Values["SelectedSpeakerName"];
            string preferredOutputAudioDeviceId = string.Empty;

            for (int i = 0; i < Devices.Instance.AudioMediaDevicesRendersList.Count; i++)
            {
                if (selectedSpeakerName == Devices.Instance.AudioMediaDevicesRendersList[i].DisplayName)
                    preferredOutputAudioDeviceId = Devices.Instance.AudioMediaDevicesRendersList[i].Id;
            }

            return preferredOutputAudioDeviceId;
        }

        private string GetPreferredInputAudioDeviceId()
        {
            string selectedMicrophoneName = (string)localSettings.Values["SelectedMicrophoneName"];
            string preferredInputAudioDeviceId = string.Empty;

            for (int i = 0; i < Devices.Instance.AudioMediaDevicesCapturersList.Count; i++)
            {
                if (selectedMicrophoneName == Devices.Instance.AudioMediaDevicesCapturersList[i].DisplayName)
                    preferredInputAudioDeviceId = Devices.Instance.AudioMediaDevicesCapturersList[i].Id;
            }

            return preferredInputAudioDeviceId;
        }

        private string GetPreferredVideoCodecId()
        {
            string selectedVideoCodecName = (string)localSettings.Values["SelectedVideoCodecName"];
            string preferredVideoCodecId = string.Empty;
            
            // TODO: get video codecs list
            // from RTCRtpCapabilities getCapabilities(WebRtcFactory factory, string kind);
            // set preferredVideoCodecId

            return preferredVideoCodecId;
        }

        private string GetPreferredAudioCodecId()
        {
            string selectedAudioCodecName = (string)localSettings.Values["SelectedAudioCodecName"];
            string preferredAudioCodecId = string.Empty;

            //for (int i = 0; i < DefaultSettings.GetAudioCodecs.Count; i++)
            //{


            //    if (selectedAudioCodecName == DefaultSettings.GetAudioCodecs[i].Name)
            //        preferredAudioCodecId = DefaultSettings.GetAudioCodecs[i].Id;
            //}

            return preferredAudioCodecId;
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

            GetCodecs();

            return config;
        }

        public Dictionary<string, string> VideoCodecsDict = new Dictionary<string, string>();
        public void GetCodecs()
        {
            //RTCRtpCapabilities audioCapabilities = RTCRtpSender.GetCapabilities(_factory, "audio");
            //IReadOnlyList<RTCRtpCodecCapability> audioCodecs = audioCapabilities.Codecs;
            //foreach (var item in audioCodecs)
            //    Debug.WriteLine("!!!audio codecs: " + item.Name);

            RTCRtpCapabilities videoCapabilities = RTCRtpSender.GetCapabilities(_factory, "video");
            IReadOnlyList<RTCRtpCodecCapability> videoCodecs = videoCapabilities.Codecs;
            foreach (var item in videoCodecs)
            {
                string payload = item.PreferredPayloadType.ToString();
                VideoCodecsDict.Add(payload, item.Name + " " + payload);
            }
        }

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

        private void GetUserMedia()
        {
            Debug.WriteLine("Getting user media.");

            IReadOnlyList<IConstraint> mandatoryConstraints = new List<IConstraint>();
            //{
            //    new Constraint("maxWidth", Devices.Instance.VideoCaptureProfile.Width.ToString()),
            //    new Constraint("minWidth", Devices.Instance.VideoCaptureProfile.Width.ToString()),
            //    new Constraint("maxHeight", Devices.Instance.VideoCaptureProfile.Height.ToString()),
            //    new Constraint("minHeight", Devices.Instance.VideoCaptureProfile.Height.ToString()),
            //    new Constraint("maxFrameRate", Devices.Instance.VideoCaptureProfile.FrameRate.ToString()),
            //    new Constraint("minFrameRate", Devices.Instance.VideoCaptureProfile.FrameRate.ToString())
            //};

            IReadOnlyList<IConstraint> optionalConstraints = new List<IConstraint>();

            MediaDevice _selectedVideoDevice = Devices.Instance.VideoMediaDevicesList[0];

            for (int i = 0; i < Devices.Instance.VideoMediaDevicesList.Count; i++)
                if (Devices.Instance.VideoMediaDevicesList[i].DisplayName == localSettings.Values["SelectedCameraName"]?.ToString())
                    _selectedVideoDevice = Devices.Instance.VideoMediaDevicesList[i];

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

        private void AddLocalMediaTracks()
        {
            Debug.WriteLine("Adding local media tracks.");
            PeerConnection.AddTrack(_selfVideoTrack);
            PeerConnection.AddTrack(_selfAudioTrack);

            OnAddLocalTrack?.Invoke(_selfVideoTrack);
            OnAddLocalTrack?.Invoke(_selfAudioTrack);
        }

        private void BindSelfVideo()
        {
            if (_selfVideoTrack != null)
            {

                _selfVideoTrack.Element = MediaElementMaker.Bind(SelfVideo);
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

        private bool _cameraEnabled = true;
        private bool _microphoneIsOn = true;

        /// <summary>
        /// Add local media track event handler.
        /// </summary>
        /// <param name="track">Media track kind.</param>
        public void Instance_OnAddLocalTrack(IMediaStreamTrack track)
        {
            Debug.WriteLine("Add local track!");

            if (track.Kind == "audio")
            {
                if (_microphoneIsOn)
                {
                    Debug.WriteLine("audio!");
                }
            }
            if (track.Kind == "video")
            {
                if (_cameraEnabled)
                {
                    Debug.WriteLine("video!");
                    EnableLocalVideoStream();
                }
            }
        }

        public object MediaLock { get; set; } = new object();
        private bool VideoEnabled = true;

        /// <summary>
        /// Enables the local video stream.
        /// </summary>
        private void EnableLocalVideoStream()
        {
            lock (MediaLock)
            {
                if (_selfVideoTrack != null)
                    _selfVideoTrack.Enabled = true;
                VideoEnabled = true;
            }
        }

        /// <summary>
        /// Add remote media track event handler.
        /// </summary>
        /// <param name="track">Media track kind.</param>
        public void Instance_OnAddRemoteTrack(IMediaStreamTrack track)
        {
            Debug.WriteLine("MainPage: Add remote media track!");
        }

        public event Action<IMediaStreamTrack> OnAddLocalTrack;

        private IMediaStreamTrack _peerVideoTrack;
        private IMediaStreamTrack _selfVideoTrack;
        private IMediaStreamTrack _peerAudioTrack;
        private IMediaStreamTrack _selfAudioTrack;

        /// <summary>
        /// Logs in local peer to server.
        /// </summary>
        /// <returns></returns>
        public async Task LogInToServer()
        {
            Debug.WriteLine("Connects to server.");

            await HttpSignaler.Connect(Account.ServiceUri);
        }

        /// <summary>
        /// Logs out local peer from server.
        /// </summary>
        /// <returns></returns>
        public async Task LogOutFromServer()
        {
            Debug.WriteLine("Disconnects from server.");

            await HttpSignaler.SignOut();
        }

        private int _peerId = -1;

        /// <summary>
        /// Calls to connect to the selected peer.
        /// </summary>
        /// <param name="peerId">Remote peer id.</param>
        public async Task ConnectToPeer(int peerId)
        {
            Debug.Assert(_peerId == -1);

            if (PeerConnection != null)
            {
                Debug.WriteLine("[Error] We only support connection to one peer at a time.");
                return;
            }

            if (CreatePeerConnection())
            {
                _peerId = peerId;
                var offerOptions = new RTCOfferOptions();
                offerOptions.OfferToReceiveAudio = true;
                offerOptions.OfferToReceiveVideo = true;
                IRTCSessionDescription offer = await PeerConnection.CreateOffer(offerOptions);

                var audioCodecList = DefaultSettings.GetAudioCodecs;
                var videoCodecList = DefaultSettings.GetVideoCodecs;

                for (int i = 0; i < audioCodecList.Count; i++)
                {
                    if (localSettings.Values["SelectedAudioCodecName"] != null)
                    {
                        if (audioCodecList[i].Name == localSettings.Values["SelectedAudioCodecName"].ToString())
                            AudioCodec = audioCodecList[i];
                    }
                    else
                        AudioCodec = audioCodecList.First();
                }

                for (int i = 0; i < videoCodecList.Count; i++)
                {
                    if (localSettings.Values["SelectedVideoCodecName"] != null)
                    {
                        if (videoCodecList[i].Name == localSettings.Values["SelectedVideoCodecName"].ToString())
                            VideoCodec = videoCodecList[i];
                    }
                    else
                        VideoCodec = videoCodecList.First();
                }

                // Alter sdp to force usage of selected codecs
                string modifiedSdp = offer.Sdp;
                SdpUtils.SelectCodecs(ref modifiedSdp, AudioCodec.PreferredPayloadType, VideoCodec.PreferredPayloadType);
                var sdpInit = new RTCSessionDescriptionInit();
                sdpInit.Sdp = modifiedSdp;
                sdpInit.Type = offer.SdpType;
                var modifiedOffer = new RTCSessionDescription(sdpInit);

                await PeerConnection.SetLocalDescription(modifiedOffer);

                Debug.WriteLine($"Sending offer: {modifiedOffer.Sdp}");

                SendSdp(modifiedOffer);
            }
        }

        /// <summary>
        /// Calls to disconnect from peer.
        /// </summary>
        public void DisconnectFromPeer()
        {
            SendHangupMessage();

            ClosePeerConnection();
        }

        /// <summary>
        /// Helper method to send a hangup message to a peer.
        /// </summary>
        private void SendHangupMessage()
        {
            HttpSignaler.SendToPeer(_peerId, "BYE");
        }

        /// <summary>
        /// Closes a peer connection.
        /// </summary>
        private void ClosePeerConnection()
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

            Debug.WriteLine($"Send ice candidate:\n{json?.Stringify()}");

            SendMessage(json);
        }

        /// <summary>
        /// Sends SDP message.
        /// </summary>
        /// <param name="description">RTC session description.</param>
        private void SendSdp(IRTCSessionDescription description)
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

            json = new JsonObject
            {
                { NegotiationAtributes.Type, JsonValue.CreateStringValue(messageType) },
                { NegotiationAtributes.Sdp, JsonValue.CreateStringValue(description.Sdp) }
            };

            SendMessage(json);
        }

        public void MessageFromPeerTaskRun(Message message)
        {
            int peerId = int.Parse(message.PeerId);
            string content = message.Content;

            Task.Run(async () => 
            {
                Debug.Assert(_peerId == peerId || _peerId == -1);
                Debug.Assert(content.Length > 0);

                if (_peerId != peerId && _peerId != -1)
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
                            _peerId = peerId;

                            if (!CreatePeerConnection())
                            {
                                Debug.WriteLine("Failed to initialize our PeerConnection instance");

                                await HttpSignaler.SignOut();
                                return;
                            }
                            else if (_peerId != peerId)
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
                        // Send answer
                        SendSdp(answer);
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

        private void SendMessage(IJsonValue json)
        {
            Debug.WriteLine($"Send message json: {json}");

            HttpSignaler.SendToPeer(_peerId, json.Stringify());
        }

        public void SetMedia()
        {
            // Media
            IMediaProvider mediaFactory =
                ClientCore.Factory.MediaFactory.Singleton.CreateMediaProvider();

            MediaProvider mediaProvider = (MediaProvider)mediaFactory;

            Media = (Media)mediaProvider.GetMedia();

            //Media.GetCodecsAsync(MediaKind.Audio);
            //Media.GetMediaDevicesAsync(MediaKind.Audio);
        }
    }
}
