using ClientCore.Call;
using System.Threading.Tasks;

namespace WebRtcAdapter.Call
{
    public class CallProvider : ICallProvider
    {
        public static CallProvider Create()
        {
            return new CallProvider();
        }

        public static Task<ICallInfo> PlaceCallAsync(CallConfiguration config)
        {
            //return Task.Run(() => (ICallInfo)new CallInfo());
            return null;
        }

        public static Task<ICallInfo> AnswerCallAsync(
            CallConfiguration config,
            string sdpOfRemoteParty
            )
        {
            //return Task.Run(() => (ICallInfo)new CallInfo());
            return null;
        }
    }
}
