using ClientCore.Call;
using System.Collections.Generic;

namespace GuiCore
{
    public static class DefaultSettings
    {
        public static List<IceServer> AddDefaultIceServers
            => new List<IceServer>()
            {
                new IceServer { Urls = new List<string> { "stun:stun.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun1.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun2.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun3.l.google.com:19302" } },
                new IceServer { Urls = new List<string> { "stun:stun4.l.google.com:19302" } }
            };
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
}
