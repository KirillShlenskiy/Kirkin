using System.IO;
using System.Text;

namespace Kirkin.Serialization
{
    /// <summary>
    /// Standard serializer base class.
    /// </summary>
    public abstract class Serializer
    {
        /// <summary>
        /// Default encoding: UTF8 no BOM.
        /// </summary>
        public static UTF8Encoding Encoding { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        protected const int BufferSize = 8192;

        #region Deserialization

        protected abstract T Deserialize<T>(StreamReader reader);

        /// <summary>
        /// Deserializes the contents of the given stream.
        /// </summary>
        public T Deserialize<T>(Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding, true, BufferSize, true)) {
                return Deserialize<T>(reader);
            }
        }

        /// <summary>
        /// Deserializes the given string content.
        /// </summary>
        public T Deserialize<T>(string content)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.GetBytes(content))) {
                return Deserialize<T>(ms);
            }
        }

        /// <summary>
        /// Reads the file at the given path and deserializes its contents.
        /// </summary>
        public T DeserializeFile<T>(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath)) {
                return Deserialize<T>(stream);
            }
        }

        #endregion

        #region Serialization

        protected abstract void Serialize<T>(StreamWriter writer, T value);

        /// <summary>
        /// Serializes the objects and outputs the result as a string.
        /// </summary>
        public string Serialize<T>(T value)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, value);

                stream.Position = 0;

                using (StreamReader sr = new StreamReader(stream, Encoding, false, BufferSize, true)) {
                    return sr.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Serializes the value to the given stream.
        /// </summary>
        public void Serialize<T>(Stream stream, T value)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding, BufferSize, true)) {
                Serialize(writer, value);
            }
        }

        /// <summary>
        /// Serializes the object and writes it to the file at the given path.
        /// </summary>
        public void SerializeFile<T>(string filePath, T value)
        {
            using (FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write)) {
                Serialize(stream, value);
            }
        }

        #endregion
    }
}