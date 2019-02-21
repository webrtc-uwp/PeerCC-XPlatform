using ClientCore.Call;
using ClientCore.PeerCCImpl.PeerCCWebRTCImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore.WebRTCCalling
{
    public class CallFactory
    {
        public static Task<ICallInfo> PlaceCallAsync(CallConfiguration config)
        {
            return Task.Run(() => (ICallInfo)new CallInfo());
        }
        
        public static Task<ICallInfo> AnswerCallAsync(
            CallConfiguration config,
            string sdpOfRemoteParty
            )
        {
            return Task.Run(() => (ICallInfo)new CallInfo());
        }
    }
}
