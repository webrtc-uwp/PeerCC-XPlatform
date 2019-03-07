using ClientCore.Call;
using System.Threading.Tasks;

namespace ClientCore.WebRTCCalling
{
    public class MediaProvider
    {
        static Task<IMedia> GetMediaAsync()
        {
            //return Task.Run(() => (IMedia)new Media());
            return null;
        }
    }
}
