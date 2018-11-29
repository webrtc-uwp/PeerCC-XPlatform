using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace Client_UWP.Controllers
{
    /// <summary>
    /// A singleton controller for media devices.
    /// </summary>
    public sealed class MediaDevicesController
    {
        private static MediaDevicesController instance = null;
        private static readonly object InstanceLock = new object();

        public static MediaDevicesController Instance
        {
            get
            {
                lock (InstanceLock)
                {
                    if (instance == null)
                        instance = new MediaDevicesController();

                    return instance;
                }
            }
        }

        private MediaDevicesController()
        {

        }

        /// <summary>
        /// Represents video camera capture capabilities.
        /// </summary>
        public class CaptureCapability
        {
            /// <summary>
            /// Gets the width in pixels of a video capture device capibility.
            /// </summary>
            public uint Width { get; set; }

            /// <summary>
            /// Gets the height in pixels of a video capture device capibility.
            /// </summary>
            public uint Height { get; set; }

            /// <summary>
            /// Gets the frame rate in frames per second of a video capture device capibility.
            /// </summary>
            public uint FrameRate { get; set; }

            /// <summary>
            /// Get the aspect ratio of the pixels of a video capture device capibility.
            /// </summary>
            public MediaRatio PixelAspectRatio { get; set; }

            /// <summary>
            /// Get a displayable string describing all the features of a
            /// video capture device capability. Displays resolution, frame rate,
            /// and pixel aspect ratio.
            /// </summary>
            public string FullDescription { get; set; }

            /// <summary>
            /// Get a displayable string describing the resolution of a
            /// video capture device capability.
            /// </summary>
            public string ResolutionDescription { get; set; }

            /// <summary>
            /// Get a displayable string describing the frame rate in
            // frames per second of a video capture device capability.
            /// </summary>
            public string FrameRateDescription { get; set; }
        }

        /// <summary>
        /// Represents a local media device, such as a microphone or a camera.
        /// </summary>
        public class MediaDevice
        {
            /// <summary>
            /// Gets or sets an identifier of the media device.
            /// This value defaults to a unique OS assigned identifier of the media device.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// Get or sets a displayable name that describes the media device.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the location of the media device.
            /// </summary>
            public EnclosureLocation Location { get; set; }

            /// <summary>
            /// Retrieves video capabilities for a given device.
            /// </summary>
            /// <returns>This is an asynchronous method. The result is a vector of the
            /// capabilities supported by the video device.</returns>
            public IAsyncOperation<IList<CaptureCapability>> GetVideoCaptureCapabilities()
            {
                if (Id == null)
                    return null;

                MediaCapture mediaCapture = new MediaCapture();
                MediaCaptureInitializationSettings mediaSettings =
                    new MediaCaptureInitializationSettings();
                mediaSettings.VideoDeviceId = Id;

                Task initTask = mediaCapture.InitializeAsync(mediaSettings).AsTask();
                return initTask.ContinueWith(initResult => {
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
                        uint frameRate = (uint)(property.FrameRate.Numerator /
                            property.FrameRate.Denominator);
                        capabilityList.Add(new CaptureCapability
                        {
                            Width = (uint)property.Width,
                            Height = (uint)property.Height,
                            FrameRate = frameRate,
                            FrameRateDescription = $"{frameRate} fps",
                            ResolutionDescription = $"{property.Width} x {property.Height}"
                        });
                    }
                    return capabilityList;
                }).AsAsyncOperation();
            }

            /// <summary>
            /// Gets permission from the OS to get access to a media capture device. If
            /// permissions are not enabled for the calling application, the OS will
            /// display a prompt asking the user for permission.
            /// This function must be called from the UI thread.
            /// </summary>
            public static IAsyncOperation<bool> RequestAccessForMediaCapture()
            {
                MediaCapture mediaAccessRequester = new MediaCapture();
                MediaCaptureInitializationSettings mediaSettings =
                    new MediaCaptureInitializationSettings();
                mediaSettings.AudioDeviceId = "";
                mediaSettings.VideoDeviceId = "";
                mediaSettings.StreamingCaptureMode =
                    Windows.Media.Capture.StreamingCaptureMode.AudioAndVideo;
                mediaSettings.PhotoCaptureSource =
                    Windows.Media.Capture.PhotoCaptureSource.VideoPreview;
                Task initTask = mediaAccessRequester.InitializeAsync(mediaSettings).AsTask();
                return initTask.ContinueWith(initResult => {
                    bool accessRequestAccepted = true;
                    if (initResult.Exception != null)
                    {
                        Debug.WriteLine("Failed to obtain media access permission: " + initResult.Exception.Message);
                        accessRequestAccepted = false;
                    }
                    return accessRequestAccepted;
                }).AsAsyncOperation<bool>();
            }
        }
    }
}
