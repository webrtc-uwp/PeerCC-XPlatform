using ClientCore.Call;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebRtcAdapter.Call
{
    public class Media : IMedia
    {
        IList<ICodec> codecList = new List<ICodec>();

        public Task<IList<ICodec>> GetCodecsAsync(MediaKind kind)
        {
            //Codec codec = new Codec();
            //codec.GetDisplayName("");
            //codec.GetId("");
            //codec.GetMediaKind(MediaKind.Audio);
            //codec.GetRate(111);

            //codecList.Add(codec);

            return Task.Run(() => codecList);
        }

        public Task<IList<IMediaDevice>> GetMediaDevicesAsync(MediaKind kind)
        {
            throw new NotImplementedException();
        }
    }
}
