using ClientCore.Call;

namespace WebRtcAdapter.Call
{
    public class CallInfo : ICallInfo
    {
        public ICall Call { get; private set; }

        public string Sdp { get; private set; }

        public void GetSdp(string sdp)
        {
            Sdp = sdp;
        }

        public void GetCall(Call call)
        {
            Call = call;
        }
    }
}
