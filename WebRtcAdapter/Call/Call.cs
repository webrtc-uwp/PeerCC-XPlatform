using ClientCore.Call;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WebRtcAdapter.Call
{
    public class Call : ICall
    {
        public event FrameRateChangeHandler OnFrameRateChanged;
        public event ResolutionChangeHandler OnResolutionChanged;

        public Task HangupAsync()
        {
            return Task.Run(() => Debug.WriteLine("Hangup async."));
        }
    }
}
