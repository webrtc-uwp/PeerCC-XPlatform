using System.Threading.Tasks;

namespace ClientCore.Call
{
    public interface IMediaProvider
    {
        Task<IMedia> GetMediaAsync();
    }
}
