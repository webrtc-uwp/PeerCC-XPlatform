using ClientCore.Call;

namespace WebRtcAdapter.Call
{
    public class Codec : ICodec
    {
        public MediaKind Kind { get; private set; }

        public string Id { get; private set; }

        public string DisplayName { get; private set; }

        public int Rate { get; private set; }

        public void SetMediaKind(MediaKind mediaKind)
        {
            Kind = mediaKind;
        }

        public void SetId(string id)
        {
            Id = id;
        }

        public void SetDisplayName(string displayName)
        {
            DisplayName = displayName;
        }

        public void SetRate(int rate)
        {
            Rate = rate;
        }
    }
}
