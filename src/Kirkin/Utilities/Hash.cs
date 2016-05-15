namespace Kirkin.Utilities
{
    /// <summary>
    /// Hash code generation helpers.
    /// </summary>
    public static class Hash
    {
        // Taken from Roslyn.Utilities.

        const int Multiplier = unchecked((int)0xA5555529);

        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// </summary>
        public static void Combine(ref int hash, int newKey)
        {
            hash = unchecked(hash * Multiplier + newKey);
        }

        /// <summary>
        /// Combines the given hashes according to VB's Anonymous Types rules.
        /// </summary>
        public static int Combine(int hash, int newKey)
        {
            return unchecked(hash * Multiplier + newKey);
        }

        /// <summary>
        /// Combines the given hashes according to VB's Anonymous Types rules.
        /// </summary>
        public static int Combine(int hash, int key2, int key3)
        {
            unchecked
            {
                hash = hash * Multiplier + key2;
                return hash * Multiplier + key3;
            }
        }

        /// <summary>
        /// Combines the given hashes according to VB's Anonymous Types rules.
        /// </summary>
        public static int Combine(int hash, int key2, int key3, int key4)
        {
            unchecked
            {
                hash = hash * Multiplier + key2;
                hash = hash * Multiplier + key3;
                return hash * Multiplier + key4;
            }
        }

        /// <summary>
        /// Combines the given hashes according to VB's Anonymous Types rules.
        /// </summary>
        public static int Combine(int hash, int key2, int key3, int key4, int key5)
        {
            unchecked
            {
                hash = hash * Multiplier + key2;
                hash = hash * Multiplier + key3;
                hash = hash * Multiplier + key4;
                return hash * Multiplier + key5;
            }
        }

        /// <summary>
        /// Combines the given hashes according to VB's Anonymous Types rules.
        /// </summary>
        public static int Combine(int hash, int key2, int key3, int key4, int key5, int key6)
        {
            unchecked
            {
                hash = hash * Multiplier + key2;
                hash = hash * Multiplier + key3;
                hash = hash * Multiplier + key4;
                hash = hash * Multiplier + key5;
                return hash * Multiplier + key6;
            }
        }

        /// <summary>
        /// Combines the given hashes according to VB's Anonymous Types rules.
        /// </summary>
        public static int Combine(int hash, int key2, int key3, int key4, int key5, int key6, int key7)
        {
            unchecked
            {
                hash = hash * Multiplier + key2;
                hash = hash * Multiplier + key3;
                hash = hash * Multiplier + key4;
                hash = hash * Multiplier + key5;
                hash = hash * Multiplier + key6;
                return hash * Multiplier + key7;
            }
        }

        /// <summary>
        /// Combines the given hashes according to VB's Anonymous Types rules.
        /// </summary>
        public static int Combine(int hash, int key2, int key3, int key4, int key5, int key6, int key7, int key8)
        {
            unchecked
            {
                hash = hash * Multiplier + key2;
                hash = hash * Multiplier + key3;
                hash = hash * Multiplier + key4;
                hash = hash * Multiplier + key5;
                hash = hash * Multiplier + key6;
                hash = hash * Multiplier + key7;
                return hash * Multiplier + key8;
            }
        }

        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// PERF: Do not use with enum types because that involves multiple
        /// unnecessary boxing operations. Unfortunately, we can't constrain
        /// T to "non-enum", so we'll use a more restrictive constraint.
        /// </summary>
        public static void Combine<T>(ref int currentKey, T newKeyPart) where T : class
        {
            currentKey = Combine(currentKey, newKeyPart);
        }

        /// <summary>
        /// This is how VB Anonymous Types combine hash values for fields.
        /// PERF: Do not use with enum types because that involves multiple
        /// unnecessary boxing operations. Unfortunately, we can't constrain
        /// T to "non-enum", so we'll use a more restrictive constraint.
        /// </summary>
        public static int Combine<T>(int currentKey, T newKeyPart) where T : class
        {
            int hash = unchecked(currentKey * Multiplier);

            if (newKeyPart != null) {
                return unchecked(hash + newKeyPart.GetHashCode());
            }

            return hash;
        }
    }
}