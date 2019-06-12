using ClientCore.Call;
using System.Threading.Tasks;

namespace WebRtcAdapter.Call
{
    public class CallProvider : ICallProvider
    {
        public static ICallProvider Create()
        {
            return new CallProvider();
        }

        public Task<ICallInfo> AnswerCallAsync(CallConfiguration config, string sdpOfRemoteParty)
        {
            throw new System.NotImplementedException();
        }

        public ICallInfo PlaceCall(CallConfiguration config)
        {
            return new CallInfo();
        }

        public ICall GetCall()
        {
            return new Call();
        }
    }
}
