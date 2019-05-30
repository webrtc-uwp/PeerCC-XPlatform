using ClientCore.Call;
using System.Collections.Generic;

namespace WebRtcAdapter.Call
{
    public class MediaDevice : IMediaDevice
    {
        public MediaKind Kind { get; private set; }

        public string Id { get; private set; }

        public string DisplayName { get; private set; }

        public IList<IMediaVideoFormat> VideoFormats { get; private set; }

        public void GetMediaKind(string mediaKind)
        {
            if (mediaKind.ToLower() == "audio")
                Kind = MediaKind.Audio;
            if (mediaKind.ToLower() == "video")
                Kind = MediaKind.Video;
        }

        public void GetId(string id)
        {
            Id = id;
        }

        public void GetDisplayName(string displayName)
        {
            DisplayName = displayName;
        }

        public void GetVideoFormats(IList<MediaVideoFormat> mediaVideoFormatList)
        {
            VideoFormats = new List<IMediaVideoFormat>();

            foreach (var m in mediaVideoFormatList)
                VideoFormats.Add(m);
        }
    }
}
