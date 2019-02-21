using ClientCore.Call;

namespace ClientCore.PeerCCImpl.PeerCCWebRTCImpl
{
    public class Codec : ICodec
    {
        public MediaKind Kind { get; private set; }

        public string Id { get; private set; }

        public string DisplayName { get; private set; }

        public int Rate { get; private set; }

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

        public int GetRate(int rate)
        {
            return rate;
        }
    }
}
