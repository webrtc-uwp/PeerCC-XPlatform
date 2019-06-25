using ClientCore.Account;
using ClientCore.Call;
using ClientCore.Signaling;
using GuiCore.Utilities;
using Org.WebRtc;
using PeerCC.Account;
using PeerCC.Signaling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        //private readonly object _peerConnectionLock = new object();
        //private RTCPeerConnection _peerConnection_DoNotUse;
        //public RTCPeerConnection PeerConnection
        //{
        //    get
        //    {
        //        lock (_peerConnectionLock)
        //        {
        //            return _peerConnection_DoNotUse;
        //        }
        //    }
        //    set
        //    {
        //        lock (_peerConnectionLock)
        //        {
        //            if (value == null)
        //            {
        //                if (_peerConnection_DoNotUse != null)
        //                {
        //                    (_peerConnection_DoNotUse as IDisposable)?.Dispose();
        //                }
        //            }
        //            _peerConnection_DoNotUse = value;
        //        }
        //    }
        //}

        private ApplicationDataContainer localSettings =
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
        public Codec VideoCodec { get; set; }

        /// <summary>
        /// Audio codec used in WebRTC session.
        /// </summary>
        public Codec AudioCodec { get; set; }

        private GuiLogic()
        {
            HttpSignaler = new HttpSignaler();
            _iceServers = new List<RTCIceServer>();
            PeerConnectedToServer = false;

            //HttpSignaler.MessageFromPeer += HttpSignaler_MessageFromPeer;
        }

        private void Call_OnSendMessageToRemotePeer(object sender, string e)
        {
            HttpSignaler.SendToPeer(new Message
            {
                Id = "0",
                Content = e,
                PeerId = _peerId.ToString()
            });
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

        private CallConfiguration ConfigureCall(Call call)
        {
            var callConfiguration = new CallConfiguration();

            callConfiguration.IceServers = iceServers;
            callConfiguration.PreferredInputAudioDeviceId = call.GetPreferredInputAudioDeviceId(Devices.Instance.AudioMediaDevicesCapturersList);
            callConfiguration.PreferredAudioOutputDeviceId = call.GetPreferredOutputAudioDeviceId(Devices.Instance.AudioMediaDevicesRendersList);
            callConfiguration.PreferredVideoDeviceId = call.GetPreferredVideoDeviceId(Devices.Instance.VideoMediaDevicesList);
            callConfiguration.PreferredVideoFormatId = call.GetPreferredVideoFormatId(Devices.Instance.VideoMediaDevicesList);
            callConfiguration.PreferredFrameRate = call.GetPreferredFrameRate();
            callConfiguration.PreferredAudioCodecId = call.GetPreferredAudioCodecId(Devices.Instance.AudioCodecsList);
            callConfiguration.PreferredVideoCodecId = call.GetPreferredVideoCodecId(Devices.Instance.VideoCodecsList);
            callConfiguration.LocalVideoElement = MediaElementImpl.GetMediaElement(SelfVideo);
            callConfiguration.RemoteVideoElement = MediaElementImpl.GetMediaElement(PeerVideo);

            return callConfiguration;
        }

        public CallProvider SetCallProvider()
        {
            ICallProvider callFactory =
                ClientCore.Factory.CallFactory.Singleton.CreateICallProvider();

            return (CallProvider)callFactory;
        }

        public void SetLocalCall(string sdp)
        {
            CallProvider callProvider = SetCallProvider();

            //CallInfo callInfo = (CallInfo)callProvider.PlaceCall(ConfigureCall(callProvider));

            //Call = (Call)callProvider.GetCall();
            //callInfo.SetCall(Call);
            //callInfo.SetSdp(sdp);

            Call.OnResolutionChanged += Call_OnResolutionChanged;
            Call.OnFrameRateChanged += Call_OnFrameRateChanged;

            //await Call.HangupAsync();
        }

        public void SetRemoteCall(string sdp)
        {
            CallProvider callProvider = SetCallProvider();

            //CallInfo callInfo = (CallInfo)callProvider.AnswerCall(ConfigureCall(callProvider), sdp);

            //Call = (Call)callProvider.GetCall();
            //callInfo.SetCall(Call);
            //callInfo.SetSdp(sdp);

            Call.OnResolutionChanged += Call_OnResolutionChanged;
            Call.OnFrameRateChanged += Call_OnFrameRateChanged;

            //await Call.HangupAsync();
        }

        public void Call_OnResolutionChanged(MediaDirection direction, Size dimension)
        {
            Debug.WriteLine($"Resolution changed - direction: {direction}, dimension: {dimension.Width} x {dimension.Height}");
        }

        public void Call_OnFrameRateChanged(MediaDirection direction, int frameRate)
        {
            Debug.WriteLine($"Frame rate changed - direction: {direction}, frame rate: {frameRate}");
        }

        private List<IceServer> iceServers = new List<IceServer>();
        public void SetIceServers(List<IceServer> iceServersList)
        {
            iceServers = iceServersList;
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

            return config;
        }

        /// <summary>
        /// Creates a peer connection.
        /// </summary>
        /// <returns>True if connection to a peer is successfully created.</returns>
        private bool CreatePeerConnection()
        {
            Debug.Assert(Call.PeerConnection == null);

            Debug.WriteLine("Creating peer connection.");
            Call.PeerConnection = new RTCPeerConnection(ConfigureRtc());

            OnPeerConnectionCreated?.Invoke();

            if (Call.PeerConnection == null) 
                throw new NullReferenceException("Peer connection is not created.");

            Call.PeerConnection.OnIceGatheringStateChange += PeerConnection_OnIceGatheringStateChange;
            Call.PeerConnection.OnIceConnectionStateChange += PeerConnection_OnIceConnectionStateChange;

            Call.PeerConnection.OnIceCandidate += PeerConnection_OnIceCandidate;
            Call.PeerConnection.OnTrack += PeerConnection_OnTrack;
            Call.PeerConnection.OnRemoveTrack += PeerConnection_OnRemoveTrack;

            GetUserMedia();

            AddLocalMediaTracks();

            BindSelfVideo();

            return true;
        }

        private void PeerConnection_OnIceGatheringStateChange()
        {
            Debug.WriteLine("Ice connection state change, gathering-state = "
                    + Call.PeerConnection.IceGatheringState.ToString().ToLower());
        }

        private void PeerConnection_OnIceConnectionStateChange()
        {
            Debug.WriteLine("Ice connection state change, state="
                    + (Call.PeerConnection != null ? Call.PeerConnection.IceConnectionState.ToString().ToLower() : "closed"));
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

        private void AddLocalMediaTracks()
        {
            Debug.WriteLine("Adding local media tracks.");
            Call.PeerConnection.AddTrack(_selfVideoTrack);
            Call.PeerConnection.AddTrack(_selfAudioTrack);

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

        public object MediaLock { get; set; } = new object();

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

            ICallProvider callFactory =
                ClientCore.Factory.CallFactory.Singleton.CreateICallProvider();

            CallProvider callProvider = (CallProvider)callFactory;

            Call = (Call)callProvider.GetCallAsync();

            Call.OnSendMessageToRemotePeer += Call_OnSendMessageToRemotePeer;

            //if (Call.PeerConnection != null)
            //{
            //    Debug.WriteLine("[Error] We only support connection to one peer at a time.");
            //    return;
            //}

            //if (CreatePeerConnection())
            //{
                _peerId = peerId;

                CallInfo callInfo = (CallInfo)await Call.PlaceCallAsync(ConfigureCall(Call));


                //HttpSignaler.SendToPeer(new Message
                //{
                //    Id = "0",
                //    Content = callInfo.JsonString,
                //    PeerId = _peerId.ToString()
                //});
            //}
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
        /// Closes a peer connection.
        /// </summary>
        private void ClosePeerConnection()
        {
            lock (MediaLock)
            {
                if (Call.PeerConnection != null)
                {
                    _peerId = -1;

                    Call.PeerConnection.OnIceCandidate -= PeerConnection_OnIceCandidate;
                    Call.PeerConnection.OnTrack -= PeerConnection_OnTrack;
                    Call.PeerConnection.OnRemoveTrack -= PeerConnection_OnRemoveTrack;

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

                    Call.PeerConnection = null;

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

                if (Call.PeerConnection == null)
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

                if (Call.PeerConnection != null && !string.IsNullOrEmpty(type))
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

                    await Call.PeerConnection.SetRemoteDescription(description);

                    if (messageType == RTCSdpType.Offer)
                    {
                        //SetRemoteCall(sdp);

                        var answerOptions = new RTCAnswerOptions();
                        IRTCSessionDescription answer = await Call.PeerConnection.CreateAnswer(answerOptions);
                        await Call.PeerConnection.SetLocalDescription(answer);

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

                    await Call.PeerConnection.AddIceCandidate(candidate);

                    Debug.WriteLine($"Receiving ice candidate:\n{content}");
                }
            }).Wait();
        }

        private void SendMessage(IJsonValue json)
        {
            HttpSignaler.SendToPeer(new Message
            {
                Id = "0",
                Content = json.Stringify(),
                PeerId = _peerId.ToString()
            });

            Debug.WriteLine($"Send message json: {json}");
        }

        /// <summary>
        /// Helper method to send a hangup message to a peer.
        /// </summary>
        private void SendHangupMessage()
        {
            HttpSignaler.SendToPeer(new Message
            {
                Id = "0",
                Content = "BYE",
                PeerId = _peerId.ToString()
            });
        }
    }
}
