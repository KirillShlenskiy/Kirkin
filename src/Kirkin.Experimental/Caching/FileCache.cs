using System;
using System.IO;
using System.Linq;

using Kirkin.Serialization;

namespace Kirkin.Caching
{
    /// <summary>
    /// <see cref="ICache{T}"/> implementation which uses
    /// roundtrip serialization to store cache data in files.
    /// </summary>
    internal sealed class FileCache<T>
        : CacheBase<T>
    {
        private readonly Func<T> ValueFactory;

        /// <summary>
        /// Cache file path specified when this instance was created.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Serializer implementation specified when this instance was created.
        /// </summary>
        public Serializer Serializer { get; }

        /// <summary>
        /// Creates a new <see cref="FileCache{T}"/> instance.
        /// </summary>
        /// <param name="valueFactory">Delegate invoked when a fresh value is required.</param>
        /// <param name="filePath">Cache file path.</param>
        /// <param name="serializer">Serializer implementation used by this instance.</param>
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
                // Deliberately not handling races (i.e. catching FileNotFoundException).
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

                    if (!ms.ToArray().SequenceEqual(cloneStream.ToArray())) {
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