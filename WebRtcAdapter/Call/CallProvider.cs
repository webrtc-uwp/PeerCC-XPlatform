using System.Threading.Tasks;
using ClientCore.Call;

namespace WebRtcAdapter.Call
{
    public class CallProvider : ICallProvider
    {
        public Task<ICall> GetCallAsync()
        {
            return Task.Run(() => (ICall)new Call());
        }
    }
}
