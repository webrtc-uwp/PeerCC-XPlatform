using ClientCore.Call;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCore.PeerCCImpl.PeerCCWebRTCImpl
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
