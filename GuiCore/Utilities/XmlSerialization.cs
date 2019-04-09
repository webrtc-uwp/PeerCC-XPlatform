using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace GuiCore.Utilities
{
    public static class XmlSerialization<T>
    {
        /// <summary>
        /// Serialize to Xml.
        /// </summary>
        public static string Serialize(T param)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(param.GetType());

                using (StringWriter textWriter = new StringWriter())
                {
                    serializer.Serialize(textWriter, param);

                    return textWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] XmlSerialization Serialize message: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Deserialize from Xml
        /// </summary>
        public static T Deserialize(string xml)
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] XmlSerialization Deserialize message: " + ex.Message);
                return default(T);
            }
        }
    }
}
