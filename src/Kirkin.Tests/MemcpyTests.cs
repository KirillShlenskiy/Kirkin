using NUnit.Framework;

namespace Kirkin.Tests
{
    public class MemcpyTests
    {
        [Test]
        public unsafe void Int32_8bitStorage()
        {
            Fixed8 value = new Fixed8(1);

            Assert.AreEqual(1, Memcpy.GetInt32(&value, 0));
        }

        [Test]
        public unsafe void Int32_64bitStorage()
        {
            Fixed64 value = new Fixed64(long.MaxValue);

            Assert.AreEqual(-1, Memcpy.GetInt32(&value, 0));
            Assert.AreEqual(int.MaxValue, Memcpy.GetInt32(&value, 4));
        }

        unsafe static class Memcpy
        {
            public static int GetInt32(void* ptr, int offset)
            {
                byte* s = (byte*)ptr;

                return *(int*)(s + offset);
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