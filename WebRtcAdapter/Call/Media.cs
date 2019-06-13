using ClientCore.Call;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebRtcAdapter.Call
{
    public class Media : IMedia
    {
        public Task<IList<ICodec>> GetCodecsAsync(MediaKind kind)
        {
            return null;
        }

        public Task<IList<IMediaDevice>> GetMediaDevicesAsync(MediaKind kind)
        {
            return null;
        }
    }
}
