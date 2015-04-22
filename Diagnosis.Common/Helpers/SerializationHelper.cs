using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Diagnosis.Common
{
    public static class SerializationHelper
    {
        public static string SerializeXml<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static string SerializeDC<T>(this T toSerialize)
        {
            var ds = new DataContractSerializer(toSerialize.GetType());

            using (var output = new StringWriter())
            using (var writer = new XmlTextWriter(output) { Formatting = Formatting.Indented })
            {
                ds.WriteObject(writer, toSerialize);
                return output.GetStringBuilder().ToString();
            }
        }

        public static T DeserializeDC<T>(this string xmlString)
        {
            var ds = new DataContractSerializer(typeof(T));

            using (var output = new StringReader(xmlString))
            using (var reader = new XmlTextReader(output))
            {
                return (T)ds.ReadObject(reader);
            }
        }

        public static T DeserializeXml<T>(this string xmlString)
        {
            T objectOut = default(T);
            Type outType = typeof(T);
            using (StringReader read = new StringReader(xmlString))
            {
                XmlSerializer serializer = new XmlSerializer(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    objectOut = (T)serializer.Deserialize(reader);
                    reader.Close();
                }

                read.Close();
            }
            return objectOut;
        }
    }
}