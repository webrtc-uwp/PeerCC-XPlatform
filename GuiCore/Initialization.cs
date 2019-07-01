﻿//using Org.WebRtc;
//using System;
//using Windows.UI.Core;

//namespace GuiCore
//{
//    public sealed class Initialization
//    {
//        private static Initialization instance = null;
//        private static readonly object InstanceLock = new object();

//        public static Initialization Instance
//        {
//            get
//            {
//                lock (InstanceLock)
//                {
//                    if (instance == null)
//                        instance = new Initialization();

//                    return instance;
//                }
//            }
//        }

//        private Initialization() { }

//        public event Action<bool> Initialized;

//        public void CofigureWebRtcLib(CoreDispatcher uiDispatcher)
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
