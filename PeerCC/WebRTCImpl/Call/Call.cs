using ClientCore.Call;
using System;
using System.Threading.Tasks;

namespace PeerCC.WebRTCImpl.Call
{
    public class Call : ICall
    {
        public event FrameRateChangeHandler OnFrameRateChanged;
        public event ResolutionChangeHandler OnResolutionChanged;

        public Task HangupAsync()
        {
            throw new NotImplementedException();
        }
    }
}
