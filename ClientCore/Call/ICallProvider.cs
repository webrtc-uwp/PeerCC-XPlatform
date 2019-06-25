using System.Threading.Tasks;

namespace ClientCore.Call
{
    public interface ICallProvider
    {
        ICall GetCallAsync();
    }
}
