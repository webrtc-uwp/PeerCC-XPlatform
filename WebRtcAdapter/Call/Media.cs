using ClientCore.Call;
using Org.WebRtc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;

namespace WebRtcAdapter.Call
{
    public class Media : IMedia
    {
        public Task<IList<ICodec>> GetCodecsAsync(MediaKind kind)
        {
            IList<ICodec> videoCodecsList = new List<ICodec>();
            IList<ICodec> audioCodecsList = new List<ICodec>();

            if (kind == MediaKind.AudioCodec)
            {
                return Task.Run(() =>
                {
                    RTCRtpCapabilities audioCapabilities = RTCRtpSender.GetCapabilities(new WebRtcFactory(new WebRtcFactoryConfiguration()), "audio");
                    IReadOnlyList<RTCRtpCodecCapability> audioCodecs = audioCapabilities.Codecs;
                    foreach (var item in audioCodecs)
                    {
                        string payload = item.PreferredPayloadType.ToString();

                        Codec audioCodec = new Codec();
                        audioCodec.SetMediaKind(MediaKind.AudioCodec);
                        audioCodec.SetId(payload);
                        audioCodec.SetDisplayName(item.Name + " " + payload);
                        audioCodec.SetRate((int)item.ClockRate);

                        audioCodecsList.Add(audioCodec);
                    }
                    return audioCodecsList;
                });
            }

            if (kind == MediaKind.VideoCodec)
            {
                return Task.Run(() =>
                {
                    RTCRtpCapabilities videoCapabilities = RTCRtpSender.GetCapabilities(new WebRtcFactory(new WebRtcFactoryConfiguration()), "video");
                    IReadOnlyList<RTCRtpCodecCapability> videoCodecs = videoCapabilities.Codecs;
                    foreach (var item in videoCodecs)
                    {
                        string payload = item.PreferredPayloadType.ToString();

                        Codec videoCodec = new Codec();
                        videoCodec.SetMediaKind(MediaKind.VideoCodec);
                        videoCodec.SetId(payload);
                        videoCodec.SetDisplayName(item.Name + " " + payload);
                        videoCodec.SetRate((int)item.ClockRate);

                        videoCodecsList.Add(videoCodec);
                    }

                    return videoCodecsList;
                });
            }
            else return null;
        }

        public Task<IList<IMediaDevice>> GetMediaDevicesAsync(MediaKind kind)
        {
            if (kind == MediaKind.AudioInputDevice)
            {
                return Task.Run(async () =>
                {
                    IList<IMediaDevice> audioMediaDevicesCapturersList = new List<IMediaDevice>();

                    DeviceInformationCollection audioCapturers = await DeviceInformation.FindAllAsync(Windows.Media.Devices.MediaDevice.GetAudioCaptureSelector());

                    foreach (var microphone in audioCapturers)
                    {
                        var mediaDevice = new MediaDevice();
                        mediaDevice.GetMediaKind(MediaKind.AudioInputDevice.ToString());
                        mediaDevice.GetId(microphone.Id);
                        mediaDevice.GetDisplayName(microphone.Name);

                        audioMediaDevicesCapturersList.Add(mediaDevice);
                    }
                    return audioMediaDevicesCapturersList;
                });
            }

            if (kind == MediaKind.AudioOutputDevice)
            {
                return Task.Run(async () =>
                {
                    IList<IMediaDevice> audioMediaDevicesRendersList = new List<IMediaDevice>();

                    DeviceInformationCollection audioRenders = await DeviceInformation.FindAllAsync(Windows.Media.Devices.MediaDevice.GetAudioRenderSelector());

                    foreach (var speaker in audioRenders)
                    {
                        var mediaDevice = new MediaDevice();
                        mediaDevice.GetMediaKind(MediaKind.AudioOutputDevice.ToString());
                        mediaDevice.GetId(speaker.Id);
                        mediaDevice.GetDisplayName(speaker.Name);

                        audioMediaDevicesRendersList.Add(mediaDevice);
                    }
                    return audioMediaDevicesRendersList;
                });
            }

            if (kind == MediaKind.VideoDevice)
            {
                return Task.Run(async () =>
                {
                    IList<IMediaDevice> videoMediaDevicesList = new List<IMediaDevice>();

                    IReadOnlyList<IVideoDeviceInfo> videoDevices = await VideoCapturer.GetDevices();

                    foreach (IVideoDeviceInfo videoDevice in videoDevices)
                    {
                        var mediaDevice = new MediaDevice();
                        mediaDevice.GetMediaKind(MediaKind.VideoDevice.ToString());
                        mediaDevice.GetId(videoDevice.Info.Id);
                        mediaDevice.GetDisplayName(videoDevice.Info.Name);

                        IList<MediaVideoFormat> videoFormatsList = await GetMediaVideoFormatList(videoDevice.Info.Id);

                        mediaDevice.GetVideoFormats(videoFormatsList);

                        videoMediaDevicesList.Add(mediaDevice);
                    }
                    return videoMediaDevicesList;
                });
            }
            else return null;
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
