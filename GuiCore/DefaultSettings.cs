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

        public static IList<CodecInfoModel> GetAudioCodecs
            => new List<CodecInfoModel>
            {
                new CodecInfoModel { PreferredPayloadType = 111, ClockRate = 48000, Name = "opus" },
                new CodecInfoModel { PreferredPayloadType = 103, ClockRate = 16000, Name = "ISAC" },
                new CodecInfoModel { PreferredPayloadType = 9, ClockRate = 8000, Name = "G722" },
                new CodecInfoModel { PreferredPayloadType = 102, ClockRate = 8000, Name = "ILBC" },
                new CodecInfoModel { PreferredPayloadType = 0, ClockRate = 8000, Name = "PCMU" },
                new CodecInfoModel { PreferredPayloadType = 8, ClockRate = 8000, Name = "PCMA" }
            };

        public static IList<CodecInfoModel> GetVideoCodecs
            => new List<CodecInfoModel>
            {
                new CodecInfoModel { PreferredPayloadType = 96, ClockRate = 90000, Name = "VP8" },
                new CodecInfoModel { PreferredPayloadType = 98, ClockRate = 90000, Name = "VP9" },
                new CodecInfoModel { PreferredPayloadType = 100, ClockRate = 90000, Name = "H264" }
            };
    }

    public class CodecInfoModel
    {
        public byte PreferredPayloadType { get; set; }
        public string Name { get; set; }
        public int ClockRate { get; set; }
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
