using ClientCore.Call;
using ClientCore.PeerCCImpl.PeerCCWebRTCImpl;
using ClientCore.PeerCCSignalingImpl;
using ClientCore.WebRTCCalling;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace ClientCore.PeerCCWebRTCImpl
{
    /// <summary>
    /// A singleton RTCController for WebRtc session.
    /// </summary>
    public sealed class RTCController
    {
        public HttpSignaler _httpSignaler;

        private static RTCController instance = null;
        private static readonly object InstanceLock = new object();

        public static RTCController Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new RTCController();

                    return instance;
                }
            }
        }

        private RTCController()
        { 
            _httpSignaler = (HttpSignaler)SignalerFactory.Create();
        }

        // SDP negotiation attributes
        private static readonly string kCandidateSdpMidName = "sdpMid";
        private static readonly string kCandidateSdpMlineIndexName = "sdpMLineIndex";
        private static readonly string kCandidateSdpName = "candidate";
        private static readonly string kSessionDescriptionTypeName = "type";
        private static readonly string kSessionDescriptionSdpName = "sdp";

        RTCPeerConnection _peerConnection;

        readonly List<RTCIceServer> _iceServers;

        private int _peerId = -1;

        // Public events to notify about connection status
        public event Action OnPeerConnectionCreated;

        /// <summary>
        /// Creates a peer connection.
        /// </summary>
        /// <returns>True if connection to a peer is successfully created.</returns>
        private async Task<bool> CreatePeerConnection(CancellationToken cancellationToken)
        {
            Debug.WriteLine(_peerConnection == null);

            if (cancellationToken.IsCancellationRequested) return false;

            RTCConfiguration config = new RTCConfiguration()
            {
                BundlePolicy = RTCBundlePolicy.Balanced,
                IceTransportPolicy = RTCIceTransportPolicy.All,
                IceServers = _iceServers
            };

            Debug.WriteLine("RTCController: Creating peer connection.");

            _peerConnection = new RTCPeerConnection(config);

            _peerConnection.OnIceGatheringStateChange += () =>
            {
                Debug.WriteLine("RTCController: Ice connection state change, gathering-state = " +
                    _peerConnection.IceGatheringState.ToString().ToLower());
            };

            _peerConnection.OnIceConnectionStateChange += () =>
            {
                Debug.WriteLine("RTCController: Ice connection state change, state = " +
                    (null != _peerConnection ? _peerConnection.IceConnectionState.ToString().ToLower() : "closed"));
            };

            if (_peerConnection == null)
                throw new NullReferenceException("Peer connection is not created.");

            if (cancellationToken.IsCancellationRequested) return false;

            OnPeerConnectionCreated?.Invoke();

            _peerConnection.OnIceCandidate += PeerConnection_OnIceCandidate;

            _peerConnection.OnTrack += PeerConnection_OnTrack;
            _peerConnection.OnRemoveTrack += PeerConnection_OnRemoveTrack;

            // TODO: Getting user media

            // TODO: Video, Audio

            return true;
        }

        /// <summary>
        /// Invoked when the remote peer removed a media stream from the peer connection.
        /// </summary>
        public event Action<IMediaStreamTrack> OnRemoveRemoteTrack;
        private void PeerConnection_OnRemoveTrack(IRTCTrackEvent evt)
        {
            OnRemoveRemoteTrack?.Invoke(evt.Track);
        }

        /// <summary>
        /// Invoked when the remote peer added a media stream to the peer connection.
        /// </summary>
        public event Action<IMediaStreamTrack> OnAddRemoteTrack;
        private void PeerConnection_OnTrack(IRTCTrackEvent evt)
        {
            OnAddRemoteTrack?.Invoke(evt.Track);
        }

        /// <summary>
        /// Called when WebRTC detects another ICE candidate.
        /// This candidate needs to be sent to the other peer.
        /// </summary>
        /// <param name="evt">Details about RTC Peer Connection Ice event.</param>
        private void PeerConnection_OnIceCandidate(IRTCPeerConnectionIceEvent evt)
        {
            if (evt.Candidate == null) return;

            double index = null != evt.Candidate.SdpMLineIndex ? (double)evt.Candidate.SdpMLineIndex : -1;

            JsonObject json = null;

            json = new JsonObject
            {
                { kCandidateSdpMidName, JsonValue.CreateStringValue(evt.Candidate.SdpMid) },
                { kCandidateSdpMlineIndexName, JsonValue.CreateNumberValue(index) },
                { kCandidateSdpName, JsonValue.CreateStringValue(evt.Candidate.Candidate) }
            };

            Debug.WriteLine("RTCController: Sending ice candidate:\n" + json?.Stringify());

            SendMessage(json);
        }

        /// <summary>
        /// Helper method to send a message to a peer.
        /// </summary>
        /// <param name="json">Message body.</param>
        private void SendMessage(IJsonValue json)
        {
            // Don't await, send it async.
            // var task = _signaller.SendToPeer(_peerId, json);

            _httpSignaler.SendToPeer(_peerId, json.ToString());
        }

        //private void SendMessage(JsonObject json)
        //{
        //    throw new NotImplementedException();
        //}

        CancellationTokenSource _connectToPeerCancelationTokenSource;
        Task<bool> _connectToPeerTask;

        /// <summary>
        /// Calls to connect to the selected peer.
        /// </summary>
        /// <param name="peer">Peer to connect to.</param>
        public async void ConnectToPeer(Peer peer)
        {
            Debug.Assert(peer != null);
            Debug.Assert(_peerId == -1);

            if (_peerConnection != null)
            {
                Debug.WriteLine("[Error] RTCController: We only support connecting to one peer at a time");
                return;
            }

            CallConfiguration configuration = new CallConfiguration();
            configuration.IceServers = new List<IceServer>();
            configuration.LocalVideoElement = null;
            configuration.PreferredAudioCodecId = "";
            configuration.PreferredAudioDeviceId = "";
            configuration.PreferredFrameRate = 1111;
            configuration.PreferredVideoCodecId = "";
            configuration.PreferredVideoDeviceId = "";
            configuration.PreferredVideoFormatId = "";
            configuration.RemoveVideoElement = null;

            CallInfo callInfo = (CallInfo)await CallFactory.PlaceCallAsync(configuration);

            _connectToPeerCancelationTokenSource = new System.Threading.CancellationTokenSource();
            bool connectResult = await CreatePeerConnection(_connectToPeerCancelationTokenSource.Token);
            _connectToPeerTask = null;
            _connectToPeerCancelationTokenSource.Dispose();

            if (connectResult)
            {
                _peerId = peer.Id;
                var offerOptions = new RTCOfferOptions();
                offerOptions.OfferToReceiveAudio = true;
                offerOptions.OfferToReceiveVideo = true;
                var offer = await _peerConnection.CreateOffer(offerOptions);

                // Alter sdp to force usage of selected codecs
                string modifiedSdp = offer.Sdp;

                callInfo.GetSdp(offer.Sdp);

                //SdpUtils.SelectCodecs(ref modifiedSdp, AudioCodec.PreferredPayloadType, VideoCodec.PreferredPayloadType);
                RTCSessionDescriptionInit sdpInit = new RTCSessionDescriptionInit();
                sdpInit.Sdp = modifiedSdp;
                sdpInit.Type = offer.SdpType;
                var modifiedOffer = new RTCSessionDescription(sdpInit);

                await _peerConnection.SetLocalDescription(modifiedOffer);
                Debug.WriteLine("RTCController: Sending offer:\n" + modifiedOffer.Sdp);
                SendSdp(modifiedOffer);
            }
        }

        /// <summary>
        /// Sends SDP message.
        /// </summary>
        /// <param name="description">RTC session description.</param>
        private void SendSdp(IRTCSessionDescription description)
        {
            JsonObject json = null;

            json = new JsonObject();
            string messageType = null;
            switch (description.SdpType)
            {
                case RTCSdpType.Offer: messageType = "offer"; break;
                case RTCSdpType.Answer: messageType = "answer"; break;
                case RTCSdpType.Pranswer: messageType = "pranswer"; break;
                default: Debug.Assert(false, description.SdpType.ToString()); break;
            }

            json.Add(kSessionDescriptionTypeName, JsonValue.CreateStringValue(messageType));
            json.Add(kSessionDescriptionSdpName, JsonValue.CreateStringValue(description.Sdp));

            SendMessage(json);
        }
    }
}
