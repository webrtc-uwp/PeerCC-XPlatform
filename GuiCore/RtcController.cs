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
using System.Threading;
using System.Threading.Tasks;
using WebRtcAdapter.Call;
using Windows.Data.Json;
using Windows.Storage;

namespace GuiCore
{
    public class RtcController
    {
        private static RtcController instance = null;
        private static readonly object InstanceLock = new object();

        public static RtcController Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new RtcController();

                    return instance;
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

        public class AccountModel
        {
            public string AccountName { get; set; }
            public string ServiceUri { get; set; }
            public string IdentityUri { get; set; }

            public AccountModel() { }
        }

        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public HttpSignaler _httpSignaler;

        public Account account;
        public Call call;

        private RtcController()
        {
            _httpSignaler = new HttpSignaler();

            _iceServers = new List<RTCIceServer>();

            AccountModel accountModel =
                XmlSerialization<AccountModel>.Deserialize((string)localSettings.Values["SelectedAccount"]);

            // Account 
            IAccountProvider accountFactory =
                ClientCore.Factory.SignalingFactory.Singleton.CreateIAccountProvider();

            AccountProvider accountProvider = (AccountProvider)accountFactory;

            account = (Account)accountProvider
                .GetAccount(accountModel?.ServiceUri, _httpSignaler.LocalPeer.Name, _httpSignaler);

            _httpSignaler = (HttpSignaler)account.Signaler;

            // Call
            ICallProvider callFactory =
                ClientCore.Factory.CallFactory.Singleton.CreateICallProvider();

            CallProvider callProvider = (CallProvider)callFactory;

            call = (Call)callProvider.GetCall();

            call.OnFrameRateChanged += (x, y) => { };
            call.OnResolutionChanged += (x, y) => { };

            // Media
            IMediaProvider mediaFactory =
                ClientCore.Factory.MediaFactory.Singleton.CreateMediaProvider();

            MediaProvider mediaProvider = (MediaProvider)mediaFactory;

            Media media = (Media)mediaProvider.GetMedia();

            media.GetCodecsAsync(MediaKind.Audio);
        }

        private readonly List<RTCIceServer> _iceServers;

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

        public void ConfigureIceServers(List<IceServer> iceServers)
        {
            _iceServers.Clear();

            foreach (IceServer iceServer in iceServers)
            {
                RTCIceServer server = new RTCIceServer();
                server.Urls = iceServer.Urls;
                server.Username = iceServer.Username;
                server.Credential = iceServer.Credential;
                _iceServers.Add(server);
            }
        }

        /// <summary>
        /// Creates a peer connection.
        /// </summary>
        /// <returns>True if connection to a peer is successfully created.</returns>
        public async Task<bool> CreatePeerConnection()
        {
            Debug.Assert(PeerConnection == null);

            var factoryConfig = new WebRtcFactoryConfiguration();
            WebRtcFactory factory = new WebRtcFactory(factoryConfig);

            var config = new RTCConfiguration()
            {
                Factory = factory,
                BundlePolicy = RTCBundlePolicy.Balanced,
                IceTransportPolicy = RTCIceTransportPolicy.All,
                IceServers = _iceServers
            };

            Debug.WriteLine("Creating peer connection.");
            PeerConnection = new RTCPeerConnection(config);

            if (PeerConnection == null)
            {
                throw new NullReferenceException("Peer connection is not created.");
            }

            return true;
        }

        /// <summary>
        /// Logs in local peer to server.
        /// </summary>
        /// <returns></returns>
        public async Task LogInToServer()
        {
            Debug.WriteLine("Connects to server.");

            AccountModel account =
                    XmlSerialization<AccountModel>.Deserialize((string)localSettings.Values["SelectedAccount"]);

            await _httpSignaler.Connect(account.ServiceUri);
        }

        /// <summary>
        /// Logs out local peer from server.
        /// </summary>
        /// <returns></returns>
        public async Task LogOutFromServer()
        {
            Debug.WriteLine("Disconnects from server.");

            await _httpSignaler.SignOut();
        }

        private int _peerId = -1;

        /// <summary>
        /// Calls to connect to the selected peer.
        /// </summary>
        /// <param name="peerId"></param>
        /// <returns></returns>
        public async void ConnectToPeer(int peerId)
        {
            Debug.Assert(_peerId == -1);

            if (PeerConnection != null)
            {
                Debug.WriteLine("[Error] We only support connection to one peer at a time.");
                return;
            }

            bool connectResult = await CreatePeerConnection();

            if (connectResult)
            {
                _peerId = peerId;
                var offerOptions = new RTCOfferOptions();
                offerOptions.OfferToReceiveAudio = true;
                offerOptions.OfferToReceiveVideo = true;
                IRTCSessionDescription offer = await PeerConnection.CreateOffer(offerOptions);

                var sdpInit = new RTCSessionDescriptionInit();
                sdpInit.Sdp = offer.Sdp;
                sdpInit.Type = offer.SdpType;
                var modifiedOffer = new RTCSessionDescription(sdpInit);

                await PeerConnection.SetLocalDescription(modifiedOffer);

                Debug.WriteLine($"Sending offer: {modifiedOffer.Sdp}");

                SendSdp(modifiedOffer);

                //JsonObject json = SendSdp(modifiedOffer);

                //return json.Stringify();
            }

            //return null;
        }

        //public async Task CallRemotePeer(int remotePeerId)
        //{
        //    var message = new Message();

        //    string content = await ConnectToPeer(remotePeerId);

        //    message.Id = "1";
        //    message.PeerId = remotePeerId.ToString();
        //    message.Content = content;

        //    await _httpSignaler.SentToPeerAsync(message);
        //}

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

            //return json;

            SendMessage(json);
        }

        public void MessageFromPeerTaskRun(Message message)
        {
            Debug.WriteLine("Message from peer!");
            Debug.WriteLine("Peer id: " + message.PeerId);
            Debug.WriteLine("Message id: " + message.Id);
            Debug.WriteLine("Message content: " + message.Content);

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

                            bool connectResult = await CreatePeerConnection();

                            if (!connectResult)
                            {
                                Debug.WriteLine("Failed to initialize our PeerConnection instance");

                                await _httpSignaler.SignOut();
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

            _httpSignaler.SendToPeer(_peerId, json.Stringify());
        }
    }
}
