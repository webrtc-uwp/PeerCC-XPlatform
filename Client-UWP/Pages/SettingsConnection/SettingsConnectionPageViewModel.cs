using Client_UWP.Controllers;
using Client_UWP.Models;
using Client_UWP.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Client_UWP.Pages.SettingsConnection
{
    public class SettingsConnectionPageViewModel : ViewModelBase
    {
        public SettingsConnectionPageViewModel()
        {
            if (DeserializedList() == null)
            {
                AddDefaultIceServers(IceServersList);
                SettingsController.IceServersList = SerializedList(IceServersList);
            }
            else
            {
                List<IceServer> list = DeserializedList();

                foreach (IceServer ice in list)
                    IceServersList.Add(ice);
            }
        }

        public ObservableCollection<IceServer> IceServersList { get; set; } = new ObservableCollection<IceServer>();

        public static ObservableCollection<IceServer> AddDefaultIceServers(ObservableCollection<IceServer> IceServersList)
        {
            List<IceServer> list = DefaultSettings.IceServersList;

            foreach (IceServer ice in list)
                IceServersList.Add(ice);

            return IceServersList;
        }

        public static string SerializedList(ObservableCollection<IceServer> IceServersList)
        {
            List<IceServer> iceList = IceServersList.ToList();

            XmlSerializer serializer = new XmlSerializer(typeof(List<IceServer>));

            StringWriter stringWriter = new StringWriter();

            serializer.Serialize(stringWriter, iceList);

            return stringWriter.ToString();
        }

        public static List<IceServer> DeserializedList()
        {
            StringReader stringReader = new StringReader((string)SettingsController.IceServersList);

            XmlSerializer serializer = new XmlSerializer(typeof(List<IceServer>));

            if (stringReader.ReadLine() != null)
            {
                List<IceServer> list = (List<IceServer>)serializer.Deserialize(stringReader) as List<IceServer>;
                IceServer ice = new IceServer();

                if (list.Any()) return list;
            }
            return null;
        }
    }
}
