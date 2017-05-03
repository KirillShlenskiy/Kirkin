using System;
using System.IO;
using System.Xml.Serialization;

using SystemXmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace Kirkin.Serialization
{
    internal sealed class XmlSerializer : Serializer
    {
        private static readonly XmlSerializerFactory Factory = new XmlSerializerFactory();
        private static readonly XmlSerializerNamespaces DefaultNamespaces = CreateDefaultNamespaces();

        protected override T Deserialize<T>(StreamReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            SystemXmlSerializer serializer = CreateSerializer<T>();

            return (T)serializer.Deserialize(reader);
        }

        protected override void Serialize<T>(StreamWriter writer, T value)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            SystemXmlSerializer serializer = CreateSerializer<T>();

            serializer.Serialize(writer, value, DefaultNamespaces);
        }

        private static SystemXmlSerializer CreateSerializer<T>()
        {
            // "Root" choice is justified by additional backwards compatibility. If the
            // root type is renamed, deserialization of existing content will still work.
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