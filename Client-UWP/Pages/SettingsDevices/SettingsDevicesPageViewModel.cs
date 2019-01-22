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
            if (DeserializedList() == null)
            {
                AddDefaultAudioCodecs(AudioCodecsList);
                SettingsController.AudioCodecs = SerializedList(AudioCodecsList);
            }
            else
            {
                List<AudioCodec> list = DeserializedList();

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

        public static string SerializedList(List<AudioCodec> AudioCodecsList)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<AudioCodec>));

            StringWriter stringWriter = new StringWriter();

            serializer.Serialize(stringWriter, AudioCodecsList);

            return stringWriter.ToString();
        }

        public static List<AudioCodec> DeserializedList()
        {
            StringReader stringReader;

            if (SettingsController.AudioCodecs != null)
            {
                stringReader = new StringReader((string)SettingsController.AudioCodecs);

                XmlSerializer serializer = new XmlSerializer(typeof(List<AudioCodec>));

                if (stringReader.ReadLine() != null)
                {
                    List<AudioCodec> list =
                        (List<AudioCodec>)serializer.Deserialize(stringReader) as List<AudioCodec>;

                    AudioCodec ac = new AudioCodec();

                    if (list.Any()) return list;
                }
            }
           
            return null;
        }
    }
}
