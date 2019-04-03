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

        

        

        public Call()
        {
            WebRtcLibConfiguration configuration = new WebRtcLibConfiguration();
            configuration.AudioCaptureFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("AudioCaptureProcessingQueue");
            configuration.AudioRenderFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("AudioRenderProcessingQueue");
            configuration.VideoFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("VideoFrameProcessingQueue");

            WebRtcLib.Setup(configuration);
        }

        /// <summary>
        /// Creates a peer connection.
        /// </summary>
        /// <returns>True if connection to a peer is successfully created.</returns>
        public bool CreatePeerConnection(System.Threading.CancellationToken ct)
        {
            Debug.Assert(Adapter.Instance._peerConnection == null);

            if (ct.IsCancellationRequested) return false;

            var factoryConfig = new WebRtcFactoryConfiguration();
            WebRtcFactory factory = new WebRtcFactory(factoryConfig);

            var config = new RTCConfiguration()
            {
                Factory = factory,
                BundlePolicy = RTCBundlePolicy.Balanced,
                IceTransportPolicy = RTCIceTransportPolicy.All,
                IceServers = Adapter.Instance._iceServers
            };

            Debug.WriteLine("Creating peer connection.");
            Adapter.Instance._peerConnection = new RTCPeerConnection(config);

            if (Adapter.Instance._peerConnection == null)
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
            if (Adapter.Instance._peerConnection != null)
            {
                Debug.WriteLine("[Error] We only support connecting to one peer at a time.");
                return null;
            }

            var connectToPeerCancelationTokenSource = new System.Threading.CancellationTokenSource();

            bool connectResult = CreatePeerConnection(connectToPeerCancelationTokenSource.Token);

            connectToPeerCancelationTokenSource.Dispose();

            if (connectResult)
            {
                var offerOptions = new RTCOfferOptions();
                offerOptions.OfferToReceiveAudio = true;
                offerOptions.OfferToReceiveVideo = true;
                IRTCSessionDescription offer = await Adapter.Instance._peerConnection.CreateOffer(offerOptions);

                string offerSdp = offer.Sdp;
                RTCSessionDescriptionInit sdpInit = new RTCSessionDescriptionInit();
                sdpInit.Sdp = offerSdp;
                sdpInit.Type = offer.SdpType;
                var modifiedOffer = new RTCSessionDescription(sdpInit);

                await Adapter.Instance._peerConnection.SetLocalDescription(modifiedOffer);

                Debug.WriteLine($"Sending offer: {modifiedOffer.Sdp}");

                JsonObject json = SendSdp(modifiedOffer);

                return json.Stringify();
            }

            return null;
        }

        // SDP negotiation attributes
        private static readonly string kCandidateSdpMidName = "sdpMid";
        private static readonly string kCandidateSdpMlineIndexName = "sdpMLineIndex";
        private static readonly string kCandidateSdpName = "candidate";
        private static readonly string kSessionDescriptionTypeName = "type";
        private static readonly string kSessionDescriptionSdpName = "sdp";

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
                { kSessionDescriptionTypeName, JsonValue.CreateStringValue(messageType) },
                { kSessionDescriptionSdpName, JsonValue.CreateStringValue(description.Sdp) }
            };

            return json;
        }
    }
}
