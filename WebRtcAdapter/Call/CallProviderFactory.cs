using ClientCore.Call;
using ClientCore.Factory;

namespace WebRtcAdapter.Call
{
    public class CallProviderFactory : ICallProviderFactory
    {
        public ICallProvider Create() => new CallProvider();
    }
}
