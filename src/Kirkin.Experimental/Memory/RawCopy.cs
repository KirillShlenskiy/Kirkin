namespace Kirkin.Memory
{
    /// <summary>
    /// Raw memory copy utilities.
    /// </summary>
    unsafe static class RawCopy
    {
        #region Read

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

        #endregion

        #region Write

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

        #endregion

        #region Refs (read/write)

        /// <summary>
        /// Returns a reference to the 32-bit signed int value at offset 0.
        /// </summary>
        public static ref int RefInt32(void* source) => ref *(int*)((byte*)source);

        /// <summary>
        /// Returns a reference to the 32-bit signed int value at the specified offset.
        /// </summary>
        public static ref int RefInt32(void* source, int offset) => ref *(int*)((byte*)source + offset);

        /// <summary>
        /// Returns a reference to the 32-bit unsigned int value at offset 0.
        /// </summary>
        public static ref uint RefUInt32(void* source) => ref *(uint*)((byte*)source);

        /// <summary>
        /// Returns a reference to the 32-bit unsigned int value at the specified offset.
        /// </summary>
        public static ref uint RefUInt32(void* source, int offset) => ref *(uint*)((byte*)source + offset);

        /// <summary>
        /// Returns a reference to the 64-bit signed int value at offset 0.
        /// </summary>
        public static ref long RefInt64(void* source) => ref *(long*)((byte*)source);

        /// <summary>
        /// Returns a reference to the 64-bit signed int value at the specified offset.
        /// </summary>
        public static ref long RefInt64(void* source, int offset) => ref *(long*)((byte*)source + offset);

        /// <summary>
        /// Returns a reference to the 64-bit unsigned int value at offset 0.
        /// </summary>
        public static ref ulong RefUInt64(void* source) => ref *(ulong*)((byte*)source);

        /// <summary>
        /// Returns a reference to the 64-bit unsigned int value at the specified offset.
        /// </summary>
        public static ref ulong RefUInt64(void* source, int offset) => ref *(ulong*)((byte*)source + offset);

        #endregion

        #region Byte arrays

        /// <summary>
        /// Reads the given number of bytes at offset 0.
        /// </summary>
        /// <param name="source">Source location to copy from.</param>
        /// <param name="count">Number of bytes to copy.</param>
        public static byte[] ReadBytes(void* source, int count)
        {
            byte[] result = new byte[count];

            fixed (byte* target = result) {
                CopyBytes(source, target, count);
            }

            return result;
        }

        /// <summary>
        /// Reads the given number of bytes at the specified offset.
        /// </summary>
        /// <param name="source">Source location to copy from.</param>
        /// <param name="offset">Source location read offset in bytes.</param>
        /// <param name="count">Number of bytes to copy.</param>
        public static byte[] ReadBytes(void* source, int offset, int count)
        {
            byte[] result = new byte[count];

            fixed (byte* target = result) {
                CopyBytes((byte*)source + offset, target, count);
            }

            return result;
        }

        /// <summary>
        /// Writes the given byte array at offset 0.
        /// </summary>
        /// <param name="target">Target location to copy to.</param>
        /// <param name="bytes">Bytes to copy.</param>
        public static void WriteBytes(void* target, byte[] bytes)
        {
            fixed (byte* source = bytes) {
                CopyBytes(source, target, bytes.Length);
            }
        }

        /// <summary>
        /// Writes the given byte array at the specified offset.
        /// </summary>
        /// <param name="target">Target location to copy to.</param>
        /// <param name="offset">Target location write offset in bytes.</param>
        /// <param name="bytes">Bytes to copy.</param>
        public static void WriteBytes(void* target, int offset, byte[] bytes)
        {
            fixed (byte* source = bytes) {
                CopyBytes(source, (byte*)target + offset, bytes.Length);
            }
        }

        #endregion

        #region CopyBytes

        /// <summary>
        /// Copies the given number of bytes from the source location
        /// (starting at offset 0) to the target location (starting at offset 0).
        /// Faster than PInvoking memcpy.
        /// </summary>
        /// <param name="source">Source memory location to copy from.</param>
        /// <param name="target">Target memory location to copy to.</param>
        /// <param name="count">Number of bytes to copy.</param>
        public static void CopyBytes(void* source, void* target, int count)
        {
            if (count % 4 == 0)
            {
                // Copy 32-bit chunks.
                CopyBytes_ChunkSize4(source, target, count / 4);
            }
            else
            {
                CopyBytes_ChunkSize1(source, target, count);
            }
        }

        private static void CopyBytes_ChunkSize4(void* source, void* target, int count)
        {
            int* src = (int*)source;
            int* dest = (int*)target;

            for (int i = 0; i < count; i++) {
                *dest++ = *src++;
            }
        }

        private static void CopyBytes_ChunkSize1(void* source, void* target, int count)
        {
            byte* src = (byte*)source;
            byte* dest = (byte*)target;

            for (int i = 0; i < count; i++) {
                *dest++ = * src++;
            }
        }

        /// <summary>
        /// Copies the given number of bytes from the source location (starting at
        /// the given offset) to the target location (starting at the given offset).
        /// Faster than PInvoking memcpy.
        /// </summary>
        /// <param name="source">Source memory location to copy from.</param>
        /// <param name="sourceOffset">Source location read offset in bytes.</param>
        /// <param name="target">Target memory location to copy to.</param>
        /// <param name="targetOffset">Target location write offset in bytes.</param>
        /// <param name="count">Number of bytes to copy.</param>
        public static void CopyBytes(void* source, int sourceOffset, void* target, int targetOffset, int count)
        {
            if (sourceOffset % 4 == 0 && targetOffset % 4 == 0 && count % 4 == 0)
            {
                // Copy 32-bit chunks.
                CopyBytes_ChunkSize4(source, sourceOffset / 4, target, targetOffset / 4, count / 4);
            }
            else
            {
                CopyBytes_ChunkSize1(source, sourceOffset, target, targetOffset, count);
            }
        }

        private static void CopyBytes_ChunkSize4(void* source, int sourceOffset, void* target, int targetOffset, int count)
        {
            int* src = (int*)source + sourceOffset;
            int* dest = (int*)target + targetOffset;

            for (int i = 0; i < count; i++) {
                *dest++ = *src++;
            }
        }

        private static void CopyBytes_ChunkSize1(void* source, int sourceOffset, void* target, int targetOffset, int count)
        {
            byte* src = (byte*)source + sourceOffset;
            byte* dest = (byte*)target + targetOffset;

            for (int i = 0; i < count; i++) {
                *dest++ = * src++;
            }
        }

        #endregion
    }
}