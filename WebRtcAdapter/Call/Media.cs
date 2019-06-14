using ClientCore.Call;
using Org.WebRtc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebRtcAdapter.Call
{
    public class Media : IMedia
    {
        public Task<IList<ICodec>> GetCodecsAsync(MediaKind kind)
        {
            IList<ICodec> videoCodecsList = new List<ICodec>(); 
            IList<ICodec> audioCodecsList = new List<ICodec>();

            if (kind == MediaKind.Audio)
            {
                return Task.Run(() => 
                {
                    RTCRtpCapabilities audioCapabilities = RTCRtpSender.GetCapabilities(new WebRtcFactory(new WebRtcFactoryConfiguration()), "audio");
                    IReadOnlyList<RTCRtpCodecCapability> audioCodecs = audioCapabilities.Codecs;
                    foreach (var item in audioCodecs)
                    {
                        string payload = item.PreferredPayloadType.ToString();

                        Codec audioCodec = new Codec();
                        audioCodec.SetMediaKind(MediaKind.Video);
                        audioCodec.SetId(payload);
                        audioCodec.SetDisplayName(item.Name + " " + payload);
                        audioCodec.SetRate((int)item.ClockRate);

                        audioCodecsList.Add(audioCodec);
                    }
                    return audioCodecsList;
                });
            }

            if (kind == MediaKind.Video)
            {
                return Task.Run(() =>
                {
                    RTCRtpCapabilities videoCapabilities = RTCRtpSender.GetCapabilities(new WebRtcFactory(new WebRtcFactoryConfiguration()), "video");
                    IReadOnlyList<RTCRtpCodecCapability> videoCodecs = videoCapabilities.Codecs;
                    foreach (var item in videoCodecs)
                    {
                        string payload = item.PreferredPayloadType.ToString();

                        Codec videoCodec = new Codec();
                        videoCodec.SetMediaKind(MediaKind.Video);
                        videoCodec.SetId(payload);
                        videoCodec.SetDisplayName(item.Name + " " + payload);
                        videoCodec.SetRate((int)item.ClockRate);

                        videoCodecsList.Add(videoCodec);
                    }

                    return videoCodecsList;
                });
            }
            else return null;
        }

        public Task<IList<IMediaDevice>> GetMediaDevicesAsync(MediaKind kind)
        {
            return null;
        }
    }
}
