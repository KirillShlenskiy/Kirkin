using NUnit.Framework;

namespace Kirkin.Tests
{
    public unsafe class MemcpyTests
    {
        [Test]
        public void Memcpy_RawCopy()
        {
            int a = -1;
            int b = 0;

            Memcpy.CopyBytes(&a, 0, &b, 0, sizeof(int));

            Assert.AreEqual(-1, b);
        }

        [Test]
        public void Memcpy_Bool()
        {
            bool value = true;
            int result = 0;

            Memcpy.CopyBytes(&value, 0, &result, 0, sizeof(bool));

            Assert.AreEqual(1, result);

            Memcpy.CopyBytes(&value, 0, &result, 1, sizeof(bool));

            Assert.AreEqual(257, result);
        }

        [Test]
        public void Memcpy_8bitStorage()
        {
            Fixed8 value = new Fixed8();

            Memcpy.SetInt32(&value, 0, 1);

            Assert.AreEqual(1, Memcpy.GetInt32(&value, 0));
        }

        [Test]
        public void Memcpy_32bitStorage()
        {
            Fixed32 value = new Fixed32();

            Assert.AreEqual(0, Memcpy.GetInt32(&value, 0));

            Memcpy.SetBytes(&value, 0, new byte[] { 255, 255, 255, 255 });

            Assert.AreEqual(-1, Memcpy.GetInt32(&value, 0));
        }

        [Test]
        public void Memcpy_64bitStorage()
        {
            Fixed64 value = new Fixed64(long.MaxValue);

            Assert.AreEqual(-1, Memcpy.GetInt32(&value, 0));
            Assert.AreEqual(int.MaxValue, Memcpy.GetInt32(&value, 4));

            Memcpy.SetInt32(&value, 4, -1);

            Assert.AreEqual(-1, Memcpy.GetInt64(&value, 0));
        }

        unsafe static class Memcpy
        {
            public static int GetInt32(void* source, int offset) => *(int*)((byte*)source + offset);
            public static uint GetUInt32(void* source, int offset) => *(uint*)((byte*)source + offset);
            public static long GetInt64(void* source, int offset) => *(long*)((byte*)source + offset);
            public static ulong GetUInt64(void* source, int offset) => *(ulong*)((byte*)source + offset);
            public static void SetInt32(void* target, int offset, int value) => *((int*)((byte*)target + offset)) = value;
            public static void SetUInt32(void* target, int offset, uint value) => *((uint*)((byte*)target + offset)) = value;
            public static void SetInt64(void* target, int offset, long value) => *((long*)((byte*)target + offset)) = value;
            public static void SetUInt64(void* target, int offset, ulong value) => *((ulong*)((byte*)target + offset)) = value;

            public static byte[] GetBytes(void* source, int offset, int count)
            {
                byte[] result = new byte[count];
                byte* src = (byte*)source + offset;

                for (int i = 0; i < count; i++) {
                    result[i] = *src++;
                }

                return result;
            }

            public static void SetBytes(void* source, int offset, byte[] bytes)
            {
                byte* target = (byte*)source + offset;

                for (int i = 0; i < bytes.Length; i++) {
                    *target++ = bytes[i];
                }
            }

            public static void CopyBytes(void* source, int sourceOffset, void* target, int targetOffset, int count)
            {
                byte* src = (byte*)source + sourceOffset;
                byte* tgt = (byte*)target + targetOffset;

                for (int i = 0; i < count; i++) {
                    *tgt++ = * src++;
                }
            }
        }

        unsafe struct Fixed8
        {
            private fixed byte storage[1];

            public Fixed8(int val)
            {
                fixed (byte* s = storage) {
                    *(int*)s = val;
                }
            }
        }

        unsafe struct Fixed16
        {
            private fixed byte storage[2];

            public Fixed16(int val)
            {
                fixed (byte* s = storage) {
                    *(int*)s = val;
                }
            }
        }

        unsafe struct Fixed32
        {
            private fixed byte storage[4];

            public Fixed32(int val)
            {
                fixed (byte* s = storage) {
                    *(int*)s = val;
                }
            }
        }

        unsafe struct Fixed64
        {
            private fixed byte storage[8];

            public Fixed64(long val)
            {
                fixed (byte* s = storage) {
                    *(long*)s = val;
                }
            }
        }
    }
}