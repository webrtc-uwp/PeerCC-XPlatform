using System.Threading.Tasks;
using ClientCore.Call;

namespace WebRtcAdapter.Call
{
    public class MediaProvider : IMediaProvider
    {
        public static MediaProvider Create()
        {
            return new MediaProvider();
        }

        public Task<IMedia> GetMediaAsync()
        {
            throw new System.NotImplementedException();
        }

        public IMedia GetMedia()
        {
            return new Media();
        }
    }
}
