using ClientCore.Signaling;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace ClientCore
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
            _httpSignaler = new HttpSignaler();
        }

        // SDP negotiation attributes
        private static readonly string kCandidateSdpMidName = "sdpMid";
        private static readonly string kCandidateSdpMlineIndexName = "sdpMLineIndex";
        private static readonly string kCandidateSdpName = "candidate";

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

            Debug.WriteLine("RTCManager: Creating peer connection.");

            _peerConnection = new RTCPeerConnection(config);

            _peerConnection.OnIceGatheringStateChange += () =>
            {
                Debug.WriteLine("RTCManager: Ice connection state change, gathering-state = " +
                    _peerConnection.IceGatheringState.ToString().ToLower());
            };

            _peerConnection.OnIceConnectionStateChange += () =>
            {
                Debug.WriteLine("RTCManager: Ice connection state change, state = " +
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

            Debug.WriteLine("RTCManager: Sending ice candidate:\n" + json?.Stringify());

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
    }
}
