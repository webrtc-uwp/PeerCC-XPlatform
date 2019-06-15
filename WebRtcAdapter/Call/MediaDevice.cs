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
            if (mediaKind == MediaKind.AudioInputDevice.ToString())
                Kind = MediaKind.AudioInputDevice;
            if (mediaKind == MediaKind.AudioOutputDevice.ToString())
                Kind = MediaKind.AudioOutputDevice;
            if (mediaKind == MediaKind.VideoDevice.ToString())
                Kind = MediaKind.VideoDevice;
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
