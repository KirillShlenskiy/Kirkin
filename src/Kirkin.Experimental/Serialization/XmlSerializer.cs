using System.IO;

using XSerializer = System.Xml.Serialization.XmlSerializer;

namespace Kirkin.Serialization
{
    internal sealed class XmlSerializer : Serializer
    {
        public override T Deserialize<T>(Stream stream)
        {
            XSerializer serializer = new XSerializer(typeof(T));

            return (T)serializer.Deserialize(stream);
        }

        public override void Serialize<T>(T content, Stream stream)
        {
            XSerializer serializer = new XSerializer(typeof(T));

            serializer.Serialize(stream, content);
        }
    }
}