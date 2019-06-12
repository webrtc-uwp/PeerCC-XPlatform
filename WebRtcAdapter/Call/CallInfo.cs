using ClientCore.Call;

namespace WebRtcAdapter.Call
{
    public class CallInfo : ICallInfo
    {
        public ICall Call { get; private set; }

        public string Sdp { get; private set; }

        public void SetSdp(string sdp)
        {
            Sdp = sdp;
        }

        public void SetCall(Call call)
        {
            Call = call;
        }
    }
}
