using ClientCore.Call;
using Org.WebRtc;
using System;
using System.Threading.Tasks;

namespace WebRtcAdapter.Call
{
    public class Call : ICall
    {
        public event FrameRateChangeHandler OnFrameRateChanged;
        public event ResolutionChangeHandler OnResolutionChanged;

        public Task HangupAsync()
        {
            throw new NotImplementedException();
        }

        public Call()
        {
            WebRtcLibConfiguration configuration = new WebRtcLibConfiguration();
            configuration.AudioCaptureFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("AudioCaptureProcessingQueue");
            configuration.AudioRenderFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("AudioRenderProcessingQueue");
            configuration.VideoFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("VideoFrameProcessingQueue");

            WebRtcLib.Setup(configuration);
        }
    }
}
