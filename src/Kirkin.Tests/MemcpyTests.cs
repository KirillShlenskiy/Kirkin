using NUnit.Framework;

namespace Kirkin.Tests
{
    public unsafe class MemcpyTests
    {
        [Test]
        public void Memcpy_RawCopyInt32()
        {
            int a = -1;
            int b = 0;

            Memcpy.CopyBytes(&a, &b, sizeof(int));

            Assert.AreEqual(-1, b);
        }

        [Test]
        public void Memcpy_RawCopyBool()
        {
            bool value = true;
            int result = 0;

            Memcpy.CopyBytes(&value, 0, &result, 0, sizeof(bool));

            Assert.AreEqual(1, result);

            Memcpy.CopyBytes(&value, 0, &result, 1, sizeof(bool));

            Assert.AreEqual(257, result);
        }

        [Test]
        public void Memcpy_ReadWriteInt8()
        {
            Block8 value = new Block8();

            Memcpy.WriteInt32(&value, 1);

            Assert.AreEqual(1, Memcpy.ReadInt32(&value));
        }

        [Test]
        public void Memcpy_ReadWriteInt32()
        {
            Block32 value = new Block32();

            Assert.AreEqual(0, Memcpy.ReadInt32(&value));

            Memcpy.WriteBytes(&value, new byte[] { 255, 255, 255, 255 });

            Assert.AreEqual(-1, Memcpy.ReadInt32(&value));
        }

        [Test]
        public void Memcpy_ReadWriteInt64()
        {
            Block64 value = new Block64();

            Assert.AreEqual(0, Memcpy.ReadInt64(&value));

            Memcpy.WriteInt64(&value, long.MaxValue);

            Assert.AreEqual(-1, Memcpy.ReadInt32(&value));
            Assert.AreEqual(int.MaxValue, Memcpy.ReadInt32(&value, 4));

            Memcpy.WriteInt32(&value, 4, -1);

            Assert.AreEqual(-1, Memcpy.ReadInt64(&value));
        }

        [Test]
        public void Memcpy_ReadWriteBytes()
        {
            byte[] bytes;
            int value = 0;

            bytes = Memcpy.ReadBytes(&value, sizeof(int));

            Assert.AreEqual(0, bytes[0]);
            Assert.AreEqual(0, bytes[1]);
            Assert.AreEqual(0, bytes[2]);
            Assert.AreEqual(0, bytes[3]);

            Memcpy.WriteBytes(&value, new byte[] { 255, 255, 255, 255 });

            Assert.AreEqual(-1, Memcpy.ReadInt32(&value));

            bytes = Memcpy.ReadBytes(&value, sizeof(int));

            Assert.AreEqual(255, bytes[0]);
            Assert.AreEqual(255, bytes[1]);
            Assert.AreEqual(255, bytes[2]);
            Assert.AreEqual(255, bytes[3]);
        }

        unsafe struct Block8
        {
            private fixed byte storage[1];
        }

        unsafe struct Block16
        {
            private fixed byte storage[2];
        }

        unsafe struct Block32
        {
            private fixed byte storage[4];
        }

        unsafe struct Block64
        {
            private fixed byte storage[8];
        }
    }
}