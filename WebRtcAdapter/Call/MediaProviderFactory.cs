using ClientCore.Call;
using ClientCore.Factory;

namespace WebRtcAdapter.Call
{
    public class MediaProviderFactory : IMediaProviderFactory
    {
        public IMediaProvider Create()
        {
            return new MediaProvider();
        }
    }
}
