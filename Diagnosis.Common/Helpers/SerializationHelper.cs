using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Diagnosis.Common
{
    public static class SerializationHelper
    {
        public static string SerializeXml<T>(this T toSerialize)
        {
            Contract.Requires(toSerialize != null);
            var ser = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                ser.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static string SerializeDCXml<T>(this T toSerialize)
        {
            Contract.Requires(toSerialize != null);
            var ser = new DataContractSerializer(toSerialize.GetType());

            using (var output = new StringWriter())
            using (var writer = new XmlTextWriter(output) { Formatting = Formatting.Indented })
            {
                ser.WriteObject(writer, toSerialize);
                return output.GetStringBuilder().ToString();
            }
        }

        public static string SerializeDCJson<T>(this T toSerialize)
        {
            Contract.Requires(toSerialize != null);
            var ser = new DataContractJsonSerializer(toSerialize.GetType());

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            {
                ser.WriteObject(stream, toSerialize);
                stream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        public static T DeserializeDCJson<T>(this string jsonString)
        {
            Contract.Requires(jsonString != null);

            var ser = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString ?? "")))
            {
                stream.Position = 0;
                return (T)ser.ReadObject(stream);
            }
        }
        public static T DeserializeDCXml<T>(this string xmlString)
        {
            Contract.Requires(xmlString != null);

            var ser = new DataContractSerializer(typeof(T));
            using (var output = new StringReader(xmlString))
            using (var reader = new XmlTextReader(output))
            {
                return (T)ser.ReadObject(reader);
            }
        }

        public static T DeserializeXml<T>(this string xmlString)
        {
            Contract.Requires(xmlString != null);

            var ser = new XmlSerializer(typeof(T));
            using (var output = new StringReader(xmlString))
            using (var reader = new XmlTextReader(output))
            {
                return (T)ser.Deserialize(reader);
            }
        }

        public static T DeepClone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}