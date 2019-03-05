using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace Client_UWP.Utilities
{
    internal static class XmlSerialization<T>
    {
        /// <summary>
        /// Serialize to Xml.
        /// </summary>
        public static string Serialize(T param)
        {
            XmlSerializer serializer = new XmlSerializer(param.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, param);

                return textWriter.ToString();
            }
        }

        /// <summary>
        /// Deserialize from Xml
        /// </summary>
        public static T Deserialize(string xml)
        {
            if (xml == null)
                return default(T);

            StringReader sr = new StringReader(xml);
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            if (sr.ReadLine() == null)
                return default(T);

            try
            {
                return (T)serializer.Deserialize(sr);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Deserialize exception: " + e.Message);

                if (e != null)
                {
                    return default(T);
                }
            }
            return (T)serializer.Deserialize(sr);
        }
    }
}
