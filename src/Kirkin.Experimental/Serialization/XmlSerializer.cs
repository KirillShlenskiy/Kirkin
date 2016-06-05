using System.IO;
using System.Xml.Serialization;

using XSerializer = System.Xml.Serialization.XmlSerializer;

namespace Kirkin.Serialization
{
    internal sealed class XmlSerializer : Serializer
    {
        private static readonly XmlSerializerFactory Factory = new XmlSerializerFactory();

        public override T Deserialize<T>(Stream stream)
        {
            XSerializer serializer = CreateSerializer<T>();

            return (T)serializer.Deserialize(stream);
        }

        public override void Serialize<T>(T content, Stream stream)
        {
            XSerializer serializer = CreateSerializer<T>();

            serializer.Serialize(stream, content);
        }

        private static XSerializer CreateSerializer<T>()
        {
            return Factory.CreateSerializer(typeof(T), new XmlRootAttribute("Cache"));
        }
    }
}