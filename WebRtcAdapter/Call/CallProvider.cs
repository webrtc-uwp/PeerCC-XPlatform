using ClientCore.Call;

namespace WebRtcAdapter.Call
{
    public class CallProvider : ICallProvider
    {
        public ICall GetCallAsync()
        {
            return new Call();
        }
    }
}
