using System;
using System.IO;

using Kirkin.ChangeTracking;
using Kirkin.Serialization;

namespace Kirkin.Caching.Persisted
{
    internal sealed class FileCache<T>
        : CacheBase<T>
    {
        public ICache<T> InnerCache { get; }
        public string FilePath { get; }
        public Serializer Serializer { get; }

        protected override T CreateValue()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    using (FileStream stream = File.OpenRead(FilePath)) {
                        return Serializer.Deserialize<T>(stream);
                    }
                }
                catch (FileNotFoundException)
                {
                    // Race.
                }
            }

            return InnerCache.Value;
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

                if (!PropertyValueEqualityComparer<T>.Default.Equals(newValue, clone)) {
                    throw new InvalidOperationException("Unable to persist object: roundtrip serialization validation failed.");
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