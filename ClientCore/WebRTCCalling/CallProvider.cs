using ClientCore.Call;
using System.Threading.Tasks;

namespace ClientCore.WebRTCCalling
{
    public class CallProvider
    {
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
