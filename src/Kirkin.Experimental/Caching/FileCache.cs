using System;
using System.IO;
using System.Linq;

using Kirkin.Serialization;

namespace Kirkin.Caching
{
    internal sealed class FileCache<T>
        : CacheBase<T>
    {
        private readonly Func<T> ValueFactory;
        public string FilePath { get; }
        public Serializer Serializer { get; }

        public FileCache(Func<T> valueFactory, string filePath, Serializer serializer)
        {
            ValueFactory = valueFactory;
            FilePath = filePath;
            Serializer = serializer;
        }

        protected override T CreateValue()
        {
            if (File.Exists(FilePath))
            {
                // Deliberatly not handling races (i.e. catching FileNotFoundException).
                // Two FileCache instances should never be using the same file path.
                using (FileStream stream = File.OpenRead(FilePath)) {
                    return Serializer.Deserialize<T>(stream);
                }
            }

            return ValueFactory();
        }

        protected override bool IsCurrentValueValid()
        {
            return File.Exists(FilePath);
        }

        protected override void OnInvalidate()
        {
            if (File.Exists(FilePath)) {
                File.Delete(FilePath);
            }
        }

        protected override void StoreValue(T newValue)
        {
            // Ensure the value can be roundtripped before writing it to file.
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(newValue, ms);

                ms.Position = 0;

                T clone = Serializer.Deserialize<T>(ms);

                using (MemoryStream cloneStream = new MemoryStream())
                {
                    Serializer.Serialize(clone, cloneStream);

                    if (!ms.GetBuffer().SequenceEqual(cloneStream.GetBuffer())) {
                        throw new InvalidOperationException("Unable to persist object: roundtrip serialization validation failed.");
                    }
                }

                // Finally, write to file.
                ms.Position = 0;

                using (FileStream stream = File.Open(FilePath, FileMode.Create, FileAccess.Write, FileShare.Write)) {
                    ms.CopyTo(stream);
                }
            }

            base.StoreValue(newValue);
        }
    }
}