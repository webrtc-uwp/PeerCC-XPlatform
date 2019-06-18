using System.Threading.Tasks;

namespace ClientCore.Call
{
    public interface ICallProvider
    {
        Task<ICall> GetCallAsync();
    }
}
