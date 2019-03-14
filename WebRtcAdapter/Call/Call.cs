using ClientCore.Call;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
    }
}
