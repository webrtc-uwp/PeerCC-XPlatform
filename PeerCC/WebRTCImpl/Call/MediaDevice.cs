using ClientCore.Call;
using System.Collections.Generic;

namespace PeerCC.WebRTCImpl.Call
{
    public class MediaDevice : IMediaDevice
    {
        public MediaKind Kind { get; private set; }

        public string Id { get; private set; }

        public string DisplayName { get; private set; }

        public IList<IMediaVideoFormat> VideoFormats { get; private set; }

        public MediaKind GetMediaKind(MediaKind mediaKind)
        {
            return mediaKind;
        }

        public string GetId(string id)
        {
            return id;
        }

        public string GetDisplayName(string displayName)
        {
            return displayName;
        }

        public IList<IMediaVideoFormat> GetVideoFormats(List<IMediaVideoFormat> mediaVideoFormatList)
        {
            return mediaVideoFormatList;
        }
    }
}
