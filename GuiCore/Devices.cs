using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace GuiCore
{
    public sealed class Devices
    {
        private static Devices instance = null;
        private static readonly object InstanceLock = new object();

        public static Devices Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new Devices();

                    return instance;
                }
            }
        }

        private Devices()
        {

        }

        public void Initialize()
        {

        }

        /// <summary>
        /// Gets permission from the OS to get access to a media capture device. 
        /// If prermissions are not enabled for the calling application, the OS 
        /// will display a prompt asking the user for permission.
        /// This function must be called from the UI thread.
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<bool> RequestAccessForMediaCapture()
        {
            MediaCapture mediaAccessRequester = new MediaCapture();

            MediaCaptureInitializationSettings mediaSettings =
                new MediaCaptureInitializationSettings();

            mediaSettings.AudioDeviceId = "";
            mediaSettings.VideoDeviceId = "";
            mediaSettings.StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo;
            mediaSettings.PhotoCaptureSource = PhotoCaptureSource.VideoPreview;

            Task initTask = mediaAccessRequester.InitializeAsync(mediaSettings).AsTask();

            return initTask.ContinueWith(initResult =>
            {
                bool accessRequestAccepted = true;
                if (initResult.Exception != null)
                {
                    Debug.WriteLine($"Failed to obtain access permission: {initResult.Exception.Message}");
                    accessRequestAccepted = false;
                }
                return accessRequestAccepted;
            }).AsAsyncOperation();
        }

        /// <summary>
        /// Video capture details (frame rate, resolution)
        /// </summary>
        public CaptureCapability VideoCaptureProfile;

        public class CaptureCapability
        {
            public uint Width { get; set; }
            public uint Height { get; set; }
            public uint FrameRate { get; set; }
            public bool MrcEnabled { get; set; }
            public string ResolutionDescription { get; set; }
            public string FrameRateDescription { get; set; }
        }

        public IAsyncOperation<IList<CaptureCapability>> GetVideoCapabilities(string deviceId)
        {
            var mediaCapture = new MediaCapture();
            var mediaSettings = new MediaCaptureInitializationSettings();

            mediaSettings.VideoDeviceId = deviceId;

            Task initTask = mediaCapture.InitializeAsync(mediaSettings).AsTask();

            return initTask.ContinueWith(initResult =>
            {
                if (initResult.Exception != null)
                {
                    Debug.WriteLine("Failed to initialize video device: " + initResult.Exception.Message);
                    return null;
                }
                var streamProperties =
                    mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord);

                IList<CaptureCapability> capabilityList = new List<CaptureCapability>();

                foreach (VideoEncodingProperties property in streamProperties)
                {
                    uint frameRate = property.FrameRate.Numerator / property.FrameRate.Denominator;

                    capabilityList.Add(new CaptureCapability
                    {
                        Width = property.Width,
                        Height = property.Height,
                        FrameRate = frameRate,
                        MrcEnabled = true,
                        FrameRateDescription = $"{frameRate} fps",
                        ResolutionDescription = $"{property.Width} x {property.Height}"
                    });
                }
                return capabilityList;
            }).AsAsyncOperation<IList<CaptureCapability>>();
        }

        public class MediaDeviceModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public ObservableCollection<string> CamerasList = new ObservableCollection<string>();

        public async Task<IList<MediaDeviceModel>> GetVideoCaptureDevices()
        {
            var devices = await VideoCapturer.GetDevices();

            IList<MediaDeviceModel> deviceList = new List<MediaDeviceModel>();

            foreach (var deviceInfo in devices)
            {
                deviceList.Add(new MediaDeviceModel
                {
                    Id = deviceInfo.Info.Id,
                    Name = deviceInfo.Info.Name
                });
            }

            return deviceList;
        }
    }
}
