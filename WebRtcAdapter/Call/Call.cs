using ClientCore.Call;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace WebRtcAdapter.Call
{
    public class Call : ICall
    {
        public event FrameRateChangeHandler OnFrameRateChanged;
        public event ResolutionChangeHandler OnResolutionChanged;

        public Task HangupAsync()
        {
            throw new NotImplementedException();
        }

        RTCPeerConnection _peerConnection;

        readonly List<RTCIceServer> _iceServers;

        /// <summary>
        /// Creates a peer connection.
        /// </summary>
        /// <returns>True if connection to a peer is successfully created.</returns>
        public bool CreatePeerConnection()
        {
            Debug.Assert(_peerConnection == null);

            var config = new RTCConfiguration()
            {
                BundlePolicy = RTCBundlePolicy.Balanced,
                IceTransportPolicy = RTCIceTransportPolicy.All,
                IceServers = _iceServers
            };

            Debug.WriteLine("Creating peer connection.");
            _peerConnection = new RTCPeerConnection(config);

            if (_peerConnection == null)
            {
                throw new NullReferenceException("Peer connection is not created.");
            }

            return true;
        }

        /// <summary>
        /// Calls to connect to the selected peer.
        /// </summary>
        public async Task<string> ConnectToPeer()
        {
            if (_peerConnection != null)
            {
                Debug.WriteLine("[Error] We only support connecting to one peer at a time.");
                return null;
            }

            bool connectResult = CreatePeerConnection();

            if (connectResult)
            {
                var offerOptions = new RTCOfferOptions();
                offerOptions.OfferToReceiveAudio = true;
                offerOptions.OfferToReceiveVideo = true;
                //IRTCSessionDescription offer = await _peerConnection.CreateOffer(offerOptions);

                return "Test message!";
            }

            return null;
        }
    }
}
