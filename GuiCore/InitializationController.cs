using Org.WebRtc;
using System.Diagnostics;

namespace GuiCore
{
    public sealed class InitializationController
    {
        private static InitializationController instance = null;
        private static readonly object InstanceLock = new object();

        public static InitializationController Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new InitializationController();

                    return instance;
                }
            }
        }

        private string AppVersion = "N/A";

        public void Initialize()
        {
            // Configure application version string format
            var version = Windows.ApplicationModel.Package.Current.Id.Version;
            AppVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

            Debug.WriteLine($"Application version: {AppVersion}");

            var configuration = new WebRtcLibConfiguration();
            configuration.AudioCaptureFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("AudioCaptureProcessingQueue");
            configuration.AudioRenderFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("AudioRenderProcessingQueue");
            configuration.VideoFrameProcessingQueue = EventQueue.GetOrCreateThreadQueueByName("VideoFrameProcessingQueue");

            WebRtcLib.Setup(configuration);

            // Install the signaler and the calling factories
            PeerCC.Setup.Install();
            WebRtcAdapter.Setup.Install();

            DevicesController.Instance.Initialize();
        }
    }
}
