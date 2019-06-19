using ClientCore.Call;
using Windows.Data.Json;

namespace WebRtcAdapter.Call
{
    public class CallInfo : ICallInfo
    {
        public ICall Call { get; private set; }

        public string Sdp { get; private set; }

        public JsonObject Json { get; private set; }

        public void SetCall(Call call)
        {
            Call = call;
        }

        public void SetSdp(string sdp)
        {
            Sdp = sdp;
        }

        public void SetJson(JsonObject json)
        {
            Json = json;
        }
    }
}
