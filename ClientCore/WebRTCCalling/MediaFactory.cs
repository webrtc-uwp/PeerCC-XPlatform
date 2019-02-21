using ClientCore.Call;
using ClientCore.PeerCCImpl.PeerCCWebRTCImpl;
using System.Threading.Tasks;

namespace ClientCore.WebRTCCalling
{
    public class MediaFactory
    {
        static Task<IMedia> GetMediaAsync()
        {
            return Task.Run(() => (IMedia)new Media());
        }
    }
}
