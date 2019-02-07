
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientCore.Call
{
    public interface IMedia
    {
        Task<IList<ICodec>> GetCodecsAsync(MediaKind kind);
    }
}
