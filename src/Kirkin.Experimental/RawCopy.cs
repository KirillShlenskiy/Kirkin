namespace Kirkin
{
    /// <summary>
    /// Raw memory copy utilities.
    /// </summary>
    unsafe static class RawCopy
    {
        /// <summary>
        /// Reads the 32-bit signed int value at offset 0.
        /// </summary>
        public static int ReadInt32(void* source) => *(int*)((byte*)source);

        /// <summary>
        /// Reads the 32-bit signed int value at the specified offset.
        /// </summary>
        public static int ReadInt32(void* source, int offset) => *(int*)((byte*)source + offset);

        /// <summary>
        /// Reads the 32-bit unsigned int value at offset 0.
        /// </summary>
        public static uint ReadUInt32(void* source) => *(uint*)((byte*)source);

        /// <summary>
        /// Reads the 32-bit unsigned int value at the specified offset.
        /// </summary>
        public static uint ReadUInt32(void* source, int offset) => *(uint*)((byte*)source + offset);

        /// <summary>
        /// Reads the 64-bit signed int value at offset 0.
        /// </summary>
        public static long ReadInt64(void* source) => *(long*)((byte*)source);

        /// <summary>
        /// Reads the 64-bit signed int value at the specified offset.
        /// </summary>
        public static long ReadInt64(void* source, int offset) => *(long*)((byte*)source + offset);

        /// <summary>
        /// Reads the 64-bit unsigned int value at offset 0.
        /// </summary>
        public static ulong ReadUInt64(void* source) => *(ulong*)((byte*)source);

        /// <summary>
        /// Reads the 64-bit unsigned int value at the specified offset.
        /// </summary>
        public static ulong ReadUInt64(void* source, int offset) => *(ulong*)((byte*)source + offset);

        /// <summary>
        /// Writes the given 32-bit signed int value at offset 0.
        /// </summary>
        public static void WriteInt32(void* target, int value) => *(int*)((byte*)target) = value;

        /// <summary>
        /// Writes the given 32-bit signed int value at the specified offset.
        /// </summary>
        public static void WriteInt32(void* target, int offset, int value) => *(int*)((byte*)target + offset) = value;

        /// <summary>
        /// Writes the given 32-bit unsigned int value at offset 0.
        /// </summary>
        public static void WriteUInt32(void* target, uint value) => *(uint*)((byte*)target) = value;

        /// <summary>
        /// Writes the given 32-bit unsigned int value at the specified offset.
        /// </summary>
        public static void WriteUInt32(void* target, int offset, uint value) => *(uint*)((byte*)target + offset) = value;

        /// <summary>
        /// Writes the given 64-bit signed int value at offset 0.
        /// </summary>
        public static void WriteInt64(void* target, long value) => *(long*)((byte*)target) = value;

        /// <summary>
        /// Writes the given 64-bit signed int value at the specified offset.
        /// </summary>
        public static void WriteInt64(void* target, int offset, long value) => *(long*)((byte*)target + offset) = value;

        /// <summary>
        /// Writes the given 64-bit unsigned int value at offset 0.
        /// </summary>
        public static void WriteUInt64(void* target, ulong value) => *(ulong*)((byte*)target) = value;

        /// <summary>
        /// Writes the given 64-bit unsigned int value at the specified offset.
        /// </summary>
        public static void WriteUInt64(void* target, int offset, ulong value) => *(ulong*)((byte*)target + offset) = value;

        /// <summary>
        /// Reads the given number of bytes at offset 0.
        /// </summary>
        public static byte[] ReadBytes(void* source, int count)
        {
            byte[] result = new byte[count];
            byte* src = (byte*)source;

            for (int i = 0; i < count; i++) {
                result[i] = *src++;
            }

            return result;
        }

        /// <summary>
        /// Reads the given number of bytes at the specified offset.
        /// </summary>
        public static byte[] ReadBytes(void* source, int offset, int count)
        {
            byte[] result = new byte[count];
            byte* src = (byte*)source + offset;

            for (int i = 0; i < count; i++) {
                result[i] = *src++;
            }

            return result;
        }

        /// <summary>
        /// Writes the given byte array at offset 0.
        /// </summary>
        public static void WriteBytes(void* source, byte[] bytes)
        {
            byte* target = (byte*)source;

            for (int i = 0; i < bytes.Length; i++) {
                *target++ = bytes[i];
            }
        }

        /// <summary>
        /// Writes the given byte array at the specified offset.
        /// </summary>
        public static void WriteBytes(void* source, int offset, byte[] bytes)
        {
            byte* target = (byte*)source + offset;

            for (int i = 0; i < bytes.Length; i++) {
                *target++ = bytes[i];
            }
        }

        /// <summary>
        /// Copies the given number of bytes from the source location
        /// (starting at offset 0) to the target location (starting at offset 0).
        /// Faster than PInvoking memcpy.
        /// </summary>
        public static void CopyBytes(void* source, void* target, int count)
        {
            byte* src = (byte*)source;
            byte* tgt = (byte*)target;

            for (int i = 0; i < count; i++) {
                *tgt++ = * src++;
            }
        }

        /// <summary>
        /// Copies the given number of bytes from the source location (starting at
        /// the given offset) to the target location (starting at the given offset).
        /// Faster than PInvoking memcpy.
        /// </summary>
        public static void CopyBytes(void* source, int sourceOffset, void* target, int targetOffset, int count)
        {
            byte* src = (byte*)source + sourceOffset;
            byte* tgt = (byte*)target + targetOffset;

            for (int i = 0; i < count; i++) {
                *tgt++ = * src++;
            }
        }
    }
}