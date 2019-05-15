using Client_UWP.Models;
using System.Collections.Generic;

namespace Client_UWP.Utilities
{
    public static class DefaultSettings
    {
        //// Default ICE servers
        //public static List<IceServerModel> IceServersList = new List<IceServerModel>
        //{
        //    new IceServerModel("stun.l.google.com:19302", "", ""),
        //    new IceServerModel("stun1.l.google.com:19302", "", ""),
        //    new IceServerModel("stun2.l.google.com:19302", "", ""),
        //    new IceServerModel("stun3.l.google.com:19302", "", ""),
        //    new IceServerModel("stun4.l.google.com:19302", "", "")
        //};

        // Codecs test data
        public static List<AudioCodec> AudioCodecsList = new List<AudioCodec>
        {
            new AudioCodec("opus|48000"),
            new AudioCodec("ISAC|16000"),
            new AudioCodec("ISAC|32000"),
            new AudioCodec("G722|8000"),
            new AudioCodec("ILBC|8000"),
            new AudioCodec("PCMU|8000"),
            new AudioCodec("PCMA|8000")
        };

        public static List<VideoCodec> VideoCodecsList = new List<VideoCodec>
        {
            new VideoCodec("VP8|90000"),
            new VideoCodec("VP9|90000"),
            new VideoCodec("H264|90000")
        };

        public static List<CaptureResolution> CaptureResolutionList = new List<CaptureResolution>
        {
            new CaptureResolution("640 x 480"),
            new CaptureResolution("160 x 120"),
            new CaptureResolution("176 x 144"),
            new CaptureResolution("320 x 240"),
            new CaptureResolution("352 x 288"),
            new CaptureResolution("424 x 240"),
            new CaptureResolution("640 x 360"),
            new CaptureResolution("800 x 448"),
            new CaptureResolution("800 x 600"),
            new CaptureResolution("848 x 480"),
            new CaptureResolution("960 x 720"),
            new CaptureResolution("1280 x 800"),
            new CaptureResolution("416 x 240"),
            new CaptureResolution("960 x 544")
        };

        public static List<CaptureFrameRate> CaptureFrameRate = new List<CaptureFrameRate>
        {
            new CaptureFrameRate("30 fps"),
            new CaptureFrameRate("20 fps"),
            new CaptureFrameRate("15 fps"),
            new CaptureFrameRate("10 fps"),
            new CaptureFrameRate("7 fps")
        };
    }
}
