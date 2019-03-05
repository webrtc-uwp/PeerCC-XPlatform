using Client_UWP.Controllers;
using Client_UWP.Models;
using Client_UWP.Utilities;
using System.Collections.Generic;
using Windows.Storage;

namespace Client_UWP.Pages.SettingsDevices
{
    public class SettingsDevicesPageViewModel : ViewModelBase
    {
        public ApplicationDataContainer localSettings =
            ApplicationData.Current.LocalSettings;

        public SettingsDevicesPageViewModel()
        {
            if (localSettings.Values["AudioCodecs"] == null)
            {
                AddDefaultAudioCodecs(AudioCodecsList);
                localSettings.Values["AudioCodecs"] = 
                    XmlSerialization<List<AudioCodec>>.Serialize(AudioCodecsList);
            }
            else
            {
                List<AudioCodec> acList = 
                    XmlSerialization<List<AudioCodec>>
                    .Deserialize((string)localSettings.Values["AudioCodecs"]);

                foreach (AudioCodec ac in acList)
                    AudioCodecsList.Add(ac);
            }

            if (localSettings.Values["VideoCodecs"] == null)
            {
                AddDefaultVideoCodecs(VideoCodecsList);
                localSettings.Values["VideoCodecs"] 
                    = XmlSerialization<List<VideoCodec>>.Serialize(VideoCodecsList);
            }
            else
            {
                List<VideoCodec> vcList =
                    XmlSerialization<List<VideoCodec>>
                    .Deserialize((string)localSettings.Values["VideoCodecs"]);

                foreach (VideoCodec vc in vcList)
                    VideoCodecsList.Add(vc);
            }
        }

        public List<AudioCodec> AudioCodecsList { get; set; } = new List<AudioCodec>();
        public List<VideoCodec> VideoCodecsList { get; set; } = new List<VideoCodec>();

        public static List<AudioCodec> AddDefaultAudioCodecs(List<AudioCodec> AudioCodecsList)
        {
            List<AudioCodec> list = DefaultSettings.AudioCodecsList;

            foreach (AudioCodec ac in list)
                AudioCodecsList.Add(ac);

            return AudioCodecsList;
        }

        public static List<VideoCodec> AddDefaultVideoCodecs(List<VideoCodec> VideoCodecsList)
        {
            List<VideoCodec> list = DefaultSettings.VideoCodecsList;

            foreach (VideoCodec vc in list)
                VideoCodecsList.Add(vc);

            return VideoCodecsList;
        }
    }
}
