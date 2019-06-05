using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using WebRtcAdapter.Call;
using System.Linq;

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

        private Devices() { }

        public List<MediaDevice> VideoMediaDevicesList = new List<MediaDevice>();
        public List<MediaDevice> AudioMediaDevicesCapturersList = new List<MediaDevice>();
        public List<MediaDevice> AudioMediaDevicesRendersList = new List<MediaDevice>();

        public async Task GetMediaDevices()
        {
            IReadOnlyList<IVideoDeviceInfo> videoDevices = await VideoCapturer.GetDevices();
            DeviceInformationCollection audioCapturers = await DeviceInformation.FindAllAsync(Windows.Media.Devices.MediaDevice.GetAudioCaptureSelector());
            DeviceInformationCollection audioRenders = await DeviceInformation.FindAllAsync(Windows.Media.Devices.MediaDevice.GetAudioRenderSelector());

            foreach (var microphone in audioCapturers)
            {
                var mediaDevice = new MediaDevice();
                mediaDevice.GetMediaKind(microphone.Kind.ToString());
                mediaDevice.GetId(microphone.Id);
                mediaDevice.GetDisplayName(microphone.Name);

                AudioMediaDevicesCapturersList.Add(mediaDevice);
            }

            foreach (var speaker in audioRenders)
            {
                var mediaDevice = new MediaDevice();
                mediaDevice.GetMediaKind(speaker.Kind.ToString());
                mediaDevice.GetId(speaker.Id);
                mediaDevice.GetDisplayName(speaker.Name);

                AudioMediaDevicesRendersList.Add(mediaDevice);
            }

            foreach (IVideoDeviceInfo videoDevice in videoDevices)
            {
                var mediaDevice = new MediaDevice();
                mediaDevice.GetMediaKind("Video");
                mediaDevice.GetId(videoDevice.Info.Id);
                mediaDevice.GetDisplayName(videoDevice.Info.Name);

                IList<MediaVideoFormat> videoFormatsList = await GetMediaVideoFormatList(videoDevice.Info.Id);

                mediaDevice.GetVideoFormats(videoFormatsList);

                VideoMediaDevicesList.Add(mediaDevice); 
            }
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

        public IAsyncOperation<IList<MediaVideoFormat>> GetMediaVideoFormatList(string deviceId)
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
                IReadOnlyList<IMediaEncodingProperties> streamProperties =
                    mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord);

                IList<MediaVideoFormat> mediaVideoFormatList = new List<MediaVideoFormat>();

                HashSet<string> resolutionsHashSet = new HashSet<string>();

                foreach (VideoEncodingProperties property in streamProperties)
                    resolutionsHashSet.Add($"{property.Width}x{property.Height}");

                foreach (string resolution in resolutionsHashSet)
                {
                    string[] r = resolution.Split("x");
                    int width = int.Parse(r[0]);
                    int height = int.Parse(r[1]);

                    HashSet<int> frameRateHashSet = new HashSet<int>();

                    foreach (VideoEncodingProperties property in streamProperties)
                        if (property.Width == width && property.Height == height)
                            frameRateHashSet.Add((int)(property.FrameRate.Numerator / property.FrameRate.Denominator));

                    var mediaVideoFormat = new MediaVideoFormat();
                    mediaVideoFormat.GetId(deviceId + resolution);
                    mediaVideoFormat.GetDimension(width, height);
                    mediaVideoFormat.GetFrameRates(frameRateHashSet.OrderBy(v => v).ToList());

                    mediaVideoFormatList.Add(mediaVideoFormat);
                }
                return mediaVideoFormatList;
            }).AsAsyncOperation<IList<MediaVideoFormat>>();
        }
    }
}
