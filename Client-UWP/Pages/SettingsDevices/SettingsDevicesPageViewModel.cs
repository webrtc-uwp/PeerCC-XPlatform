using Client_UWP.Controllers;
using Client_UWP.Models;
using Client_UWP.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Client_UWP.Pages.SettingsDevices
{
    public class SettingsDevicesPageViewModel : ViewModelBase
    {
        public SettingsDevicesPageViewModel()
        {
            if (XmlSerialization<List<AudioCodec>>.Deserialize((string)SettingsController.AudioCodecs) == null)
            {
                AddDefaultAudioCodecs(AudioCodecsList);
                SettingsController.AudioCodecs = XmlSerialization<List<AudioCodec>>.Serialize(AudioCodecsList);
            }
            else
            {
                List<AudioCodec> list = 
                    XmlSerialization<List<AudioCodec>>.Deserialize((string)SettingsController.AudioCodecs);

                foreach (AudioCodec ac in list)
                    AudioCodecsList.Add(ac);
            }
        }

        public List<AudioCodec> AudioCodecsList { get; set; } = new List<AudioCodec>();

        public static List<AudioCodec> AddDefaultAudioCodecs(List<AudioCodec> AudioCodecsList)
        {
            List<AudioCodec> list = DefaultSettings.AudioCodecsList;

            foreach (AudioCodec ac in list)
                AudioCodecsList.Add(ac);

            return AudioCodecsList;
        }
    }
}
