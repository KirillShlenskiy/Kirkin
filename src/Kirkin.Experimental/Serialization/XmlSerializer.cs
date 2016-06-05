using System.IO;
using System.Xml.Serialization;

using XSerializer = System.Xml.Serialization.XmlSerializer;

namespace Kirkin.Serialization
{
    internal sealed class XmlSerializer : Serializer
    {
        private static readonly XmlSerializerFactory Factory = new XmlSerializerFactory();
        private static readonly XmlSerializerNamespaces DefaultNamespaces = CreateDefaultNamespaces();

        public override T Deserialize<T>(Stream stream)
        {
            XSerializer serializer = CreateSerializer<T>();

            return (T)serializer.Deserialize(stream);
        }

        public override void Serialize<T>(T content, Stream stream)
        {
            XSerializer serializer = CreateSerializer<T>();

            serializer.Serialize(stream, content, DefaultNamespaces);
        }

        private static XSerializer CreateSerializer<T>()
        {
            return Factory.CreateSerializer(typeof(T), new XmlRootAttribute("Root"));
        }

        private static XmlSerializerNamespaces CreateDefaultNamespaces()
        {
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

            namespaces.Add(string.Empty, string.Empty);

            return namespaces;
        }
    }
}