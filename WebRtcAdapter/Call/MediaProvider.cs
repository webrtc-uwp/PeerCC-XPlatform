using System.Threading.Tasks;
using ClientCore.Call;

namespace WebRtcAdapter.Call
{
    public class MediaProvider : IMediaProvider
    {
        public Task<IMedia> GetMediaAsync()
        {
            return Task.Run(() => (IMedia)new Media());
        }
    }
}
