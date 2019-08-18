//using Org.WebRtc;
//using System;
//using Windows.UI.Core;

//namespace GuiCore
//{
//    public static class Initialization
//    {
//        public static event Action<bool> Initialized;

//        public static void CofigureWebRtcLib(CoreDispatcher uiDispatcher)
//        {
//            IEventQueue queue = EventQueueMaker.Bind(uiDispatcher);
//            var configuration = new WebRtcLibConfiguration();
//            configuration.Queue = queue;
//            configuration.AudioCaptureFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("AudioCaptureProcessingQueue");
//            configuration.AudioRenderFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("AudioRenderProcessingQueue");
//            configuration.VideoFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("VideoFrameProcessingQueue");

//            WebRtcLib.Setup(configuration);

//            Initialized?.Invoke(true);
//        }
//    }
//}
