using System.IO;

namespace Kirkin.Serialization
{
    internal abstract class Serializer
    {
        public abstract void Serialize<T>(T content, Stream stream);
        public abstract T Deserialize<T>(Stream steram);
    }
}