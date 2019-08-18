using ClientCore.Call;

namespace WebRtcAdapter.Call
{
    public class CallProvider : ICallProvider
    {
        public ICall GetCall() => new Call();
    }
}
