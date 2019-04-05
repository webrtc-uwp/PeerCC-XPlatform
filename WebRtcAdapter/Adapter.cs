using ClientCore.Call;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace WebRtcAdapter
{
    public class Adapter
    {
        private static Adapter instance = null;
        private static readonly object InstanceLock = new object();

        public static Adapter Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new Adapter();

                    return instance;
                }
            }
        }

        private Adapter()
        {
            _iceServers = new List<RTCIceServer>();
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

        public readonly List<RTCIceServer> _iceServers;

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
        public async Task<bool> CreatePeerConnection(CancellationToken ct)
        {
            Debug.Assert(PeerConnection == null);

            if (ct.IsCancellationRequested) return false;

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

        private int _peerId = -1;

        /// <summary>
        /// Calls to connect to the selected peer.
        /// </summary>
        public async Task<string> ConnectToPeer(int peerId)
        {
            if (PeerConnection != null)
            {
                Debug.WriteLine("[Error] We only support connecting to one peer at a time.");
                return null;
            }

            var connectToPeerCancelationTokenSource = new System.Threading.CancellationTokenSource();

            bool connectResult = await CreatePeerConnection(connectToPeerCancelationTokenSource.Token);

            connectToPeerCancelationTokenSource.Dispose();

            if (connectResult)
            {
                _peerId = peerId;
                var offerOptions = new RTCOfferOptions();
                offerOptions.OfferToReceiveAudio = true;
                offerOptions.OfferToReceiveVideo = true;
                IRTCSessionDescription offer = await PeerConnection.CreateOffer(offerOptions);

                string offerSdp = offer.Sdp;
                RTCSessionDescriptionInit sdpInit = new RTCSessionDescriptionInit();
                sdpInit.Sdp = offerSdp;
                sdpInit.Type = offer.SdpType;
                var modifiedOffer = new RTCSessionDescription(sdpInit);

                await PeerConnection.SetLocalDescription(modifiedOffer);

                Debug.WriteLine($"Sending offer: {modifiedOffer.Sdp}");

                JsonObject json = SendSdp(modifiedOffer);

                return json.Stringify();
            }

            return null;
        }

        /// <summary>
        /// Sends SDP message.
        /// </summary>
        /// <param name="description">RTC session description.</param>
        private JsonObject SendSdp(IRTCSessionDescription description)
        {
            JsonObject json = new JsonObject();
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
                { "type", JsonValue.CreateStringValue(messageType) },
                { "sdp", JsonValue.CreateStringValue(description.Sdp) }
            };

            return json;
        }

        public void MessageFromPeerTaskRun(int peerId, string message)
        {
            Task.Run(async () =>
            {
                Debug.Assert(_peerId == peerId || _peerId == -1);
                Debug.Assert(message.Length > 0);

                if (_peerId != peerId || _peerId == -1)
                {
                    Debug.WriteLine("Received a message from unknown peer " +
                        "while already in a conversation with a different peer.");

                    return;
                }

                if (!JsonObject.TryParse(message, out JsonObject jMessage))
                {
                    Debug.WriteLine($"Received unknown message: {message}");
                    return;
                }

                string type = jMessage.ContainsKey("type") ? jMessage.GetNamedString("type") : null;

                if (PeerConnection == null)
                {
                    if (!string.IsNullOrEmpty(type))
                    {
                        // Create the peer connection only when call is 
                        // about to get initiated. Otherwise ignore the 
                        // messages from peers which dould be result 
                        // of old (but not yet fully closed) connections.
                        if (type == "offer" || type == "answer" || type == "json")
                        {
                            Debug.Assert(_peerId == -1);
                            _peerId = peerId;

                            var connectToPeerCancelationTokenSource = new CancellationTokenSource();
                            bool connectResult = await CreatePeerConnection(connectToPeerCancelationTokenSource.Token);

                            connectToPeerCancelationTokenSource.Dispose();

                            if (!connectResult)
                            {
                                Debug.WriteLine("Failed to initialize our PeerConnection instance");

                                //await Signaller.SignOut();
                                return;
                            }
                            else if (_peerId != peerId)
                            {
                                Debug.WriteLine("Received a message from unknown peer while already " +
                                    "in a convesation with a different peer.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Received an untyped message after closing peer connection.");
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

                    sdp = jMessage.ContainsKey("sdp") ? jMessage.GetNamedString("sdp") : null;

                    if (string.IsNullOrEmpty(sdp))
                    {
                        Debug.WriteLine("Can't parse received session description message.");

                        return;
                    }

                    Debug.WriteLine($"Received session description:\n{message}");

                    RTCSdpType messageType = RTCSdpType.Offer;
                    switch (type)
                    {
                        case "offer": messageType = RTCSdpType.Offer; break;
                        case "answer": messageType = RTCSdpType.Answer; break;
                        case "pranswer": messageType = RTCSdpType.Pranswer; break;
                        default: Debug.Assert(false, type); break;
                    }

                    RTCSessionDescriptionInit sdpInit = new RTCSessionDescriptionInit();
                    sdpInit.Sdp = sdp;
                    sdpInit.Type = messageType;
                    var description = new RTCSessionDescription(sdpInit);

                    await PeerConnection.SetRemoteDescription(description);

                    if (messageType == RTCSdpType.Offer)
                    {
                        RTCAnswerOptions answerOptions = new RTCAnswerOptions();
                        var answer = await PeerConnection.CreateAnswer(answerOptions);
                        await PeerConnection.SetLocalDescription(answer);
                        // Send answer
                        SendSdp(answer);
                    }
                }
                else
                {
                    RTCIceCandidate candidate = null;

                    string sdpMid = jMessage.ContainsKey("sdpMid")
                    ? jMessage.GetNamedString("sdpMid") 
                    : null;
                    double sdpMlineIndex = jMessage.ContainsKey("sdpMLineIndex")
                    ? jMessage.GetNamedNumber("sdpMLineIndex")
                    : -1;
                    string sdp = jMessage.ContainsKey("candidate")
                    ? jMessage.GetNamedString("candidate")
                    : null;

                    if (string.IsNullOrEmpty(sdpMid) || sdpMlineIndex == -1 || string.IsNullOrEmpty(sdp))
                    {
                        Debug.WriteLine($"[Error] Can't parse received message.\n{message}");
                        return;
                    }

                    RTCIceCandidateInit candidateInit = new RTCIceCandidateInit();
                    candidateInit.Candidate = sdp;
                    candidateInit.SdpMid = sdpMid;
                    candidateInit.SdpMLineIndex = (ushort)sdpMlineIndex;
                    candidate = new RTCIceCandidate(candidateInit);

                    await PeerConnection.AddIceCandidate(candidate);

                    Debug.WriteLine($"Receiving ice candidate:\n{message}");
                }
            }).Wait();
        }

        private void SendMessage(IJsonValue json)
        {
            Debug.WriteLine($"[MainPage] SendMessage {json}");

            // Don't await, send it async
            //var task = _httpSignaler.SendToPeer(_peerId, json);
        }
    }
}
