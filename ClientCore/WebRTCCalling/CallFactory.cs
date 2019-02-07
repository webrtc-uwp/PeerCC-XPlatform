using ClientCore.Call;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore.WebRTCCalling
{
    public class CallFactory
    {
        static Task<ICallInfo> PlaceCallAsync(CallConfiguration config)
        {
            return null;
        }
        
        static Task<ICallInfo> AnswerCallAsync(
            CallConfiguration config,
            string sdpOfRemoteParty
            )
        {
            return null;
        }
    }
}
