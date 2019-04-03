using ClientCore.Call;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public RTCPeerConnection _peerConnection;

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

        public void MessageFromPeerTaskRun(string message)
        {
            Task.Run(async () =>
            {
                Debug.Assert(message.Length > 0);

                if (!JsonObject.TryParse(message, out JsonObject jMessage))
                {
                    Debug.WriteLine($"Unknown message {message}");
                    return;
                }

                string type = jMessage.ContainsKey("type") ? jMessage.GetNamedString("type") : null;

                if (_peerConnection == null)
                {
                    if (!string.IsNullOrEmpty(type))
                    {
                        // Create the peer connection only when call is 
                        // about to get initiated. Otherwise ignore the 
                        // messages from peers which dould be result 
                        // of old (but not yet fully closed) connections.
                        if (type == "offer" || type == "answer" || type == "json")
                        {

                        }
                    }
                    else
                    {
                        Debug.WriteLine("Received an untyped message after closing peer connection.");
                        return;
                    }
                }

                if (_peerConnection != null && !string.IsNullOrEmpty(type))
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

                    await _peerConnection.SetRemoteDescription(description);

                    if (messageType == RTCSdpType.Offer)
                    {
                        RTCAnswerOptions answerOptions = new RTCAnswerOptions();
                        var answer = await _peerConnection.CreateAnswer(answerOptions);
                        await _peerConnection.SetLocalDescription(answer);
                        // Send answer
                        SendSdp(answer);
                    }
                }
            });
        }

        private void SendSdp(IRTCSessionDescription description)
        {
            JsonObject json = null;
            Debug.WriteLine($"Sent session description: {description}");

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
                { "type", JsonValue.CreateStringValue(messageType) },
                { "sdp", JsonValue.CreateStringValue(description.Sdp) }
            };

            SendMessage(json);
        }

        private void SendMessage(IJsonValue json)
        {
            Debug.WriteLine($"[MainPage] SendMessage {json}");

            // Don't await, send it async
            //var task = _httpSignaler.SendToPeer(_peerId, json);
        }
    }
}
