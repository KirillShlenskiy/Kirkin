using System;
using System.Runtime.InteropServices;

using Kirkin.Memory;

using NUnit.Framework;

namespace Kirkin.Tests.Memory
{
    public unsafe class RawCopyTests
    {
        [Test]
        public void RawCopy_Int32()
        {
            int a = -1;
            int b = 0;

            RawCopy.CopyBytes(&a, &b, sizeof(int));

            Assert.AreEqual(-1, b);
        }

        [Test]
        public void RawCopy_Bool()
        {
            bool value = true;
            int result = 0;

            RawCopy.CopyBytes(&value, 0, &result, 0, sizeof(bool));

            Assert.AreEqual(1, result);

            RawCopy.CopyBytes(&value, 0, &result, 1, sizeof(bool));

            Assert.AreEqual(257, result);
        }

        [Test]
        public void ReadWrite_Int8()
        {
            Block8 value = new Block8();

            RawCopy.RefInt32(&value) = 1;

            Assert.AreEqual(1, RawCopy.RefInt32(&value));
        }

        [Test]
        public void ReadWrite_Int32()
        {
            Block32 value = new Block32();

            Assert.AreEqual(0, RawCopy.RefInt32(&value));

            RawCopy.WriteBytes(&value, new byte[] { 255, 255, 255, 255 });

            Assert.AreEqual(-1, RawCopy.RefInt32(&value));
        }

        [Test]
        public void ReadWrite_Int64()
        {
            Block64 value = new Block64();

            Assert.AreEqual(0, RawCopy.RefInt64(&value));

            RawCopy.RefInt64(&value) = long.MaxValue;

            Assert.AreEqual(-1, RawCopy.RefInt32(&value));
            Assert.AreEqual(int.MaxValue, RawCopy.RefInt32(&value, 4));

            RawCopy.RefInt32(&value, 4) = -1;

            Assert.AreEqual(-1, RawCopy.RefInt64(&value));
        }

        [Test]
        public void ReadWrite_Bytes()
        {
            byte[] bytes;
            int value = 0;

            bytes = RawCopy.ReadBytes(&value, sizeof(int));

            Assert.AreEqual(0, bytes[0]);
            Assert.AreEqual(0, bytes[1]);
            Assert.AreEqual(0, bytes[2]);
            Assert.AreEqual(0, bytes[3]);

            RawCopy.WriteBytes(&value, new byte[] { 255, 255, 255, 255 });

            Assert.AreEqual(-1, RawCopy.RefInt32(&value));

            bytes = RawCopy.ReadBytes(&value, sizeof(int));

            Assert.AreEqual(255, bytes[0]);
            Assert.AreEqual(255, bytes[1]);
            Assert.AreEqual(255, bytes[2]);
            Assert.AreEqual(255, bytes[3]);
        }

        [Test]
        public void RawCopy_BoxedInt32()
        {
            object a = -1;
            object b = 0;

            using (Pinned pinnedA = new Pinned(a))
            using (Pinned pinnedB = new Pinned(b)) {
                RawCopy.CopyBytes(pinnedA.Pointer, pinnedB.Pointer, sizeof(int));
            }

            Assert.AreEqual(-1, b);
            Assert.AreNotSame(a, b);
        }

        [Test]
        public void MutateString()
        {
            string s1 = "Hello world";
            string s2 = "zzz";

            fixed (char* c1 = s1, c2 = s2) {
                RawCopy.CopyBytes(c2, c1, 3 * sizeof(char));
            }

            Assert.AreEqual("zzzlo world", s1);
        }

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern void* memcpy(void* dest, void* src, int count);

        [Test]
        public void Memcpy_Int32Copy()
        {
            int a = -1;
            int b = 0;

            memcpy(&b, &a, sizeof(int));

            Assert.AreEqual(-1, b);
        }

        [Test]
        public void Benchmark_RawCopy()
        {
            int a = -1;
            int b = 0;

            for (int i = 0; i < 10000000; i++) {
                RawCopy.CopyBytes(&a, &b, sizeof(int));
            }
        }

        [Test]
        public void Benchmark_PointerCopy()
        {
            int a = -1;
            int b = 0;

            for (int i = 0; i < 10000000; i++) {
                *(&b) = *(&a);
            }
        }

        [Test]
        public void Benchmark_DirectAssignment()
        {
            int a = -1;
            int b = 0;

            for (int i = 0; i < 10000000; i++) {
                b = a;
            }
        }

        [Test]
        public void Benchmark_memcpy()
        {
            int a = -1;
            int b = 0;

            for (int i = 0; i < 10000000; i++) {
                memcpy(&b, &a, sizeof(int));
            }
        }

        static int[] numbers = new int[10000];

        [Test]
        public void Benchmark_Array_BlockCopy()
        {
            int[] target = new int[numbers.Length];

            for (int i = 0; i < 10000; i++) {
                Buffer.BlockCopy(numbers, 0, target, 0, sizeof(int) * numbers.Length);
            }
        }

        [Test]
        public void Benchmark_Array_Copy()
        {
            int[] target = new int[numbers.Length];

            for (int i = 0; i < 10000; i++) {
                Array.Copy(numbers, target, numbers.Length);
            }
        }

        [Test]
        public void Benchmark_Array_RawCopy()
        {
            int[] target = new int[numbers.Length];

            for (int i = 0; i < 10000; i++)
            {
                fixed (void* s = numbers, t = target) {
                    RawCopy.CopyBytes(s, t, sizeof(int) * numbers.Length);
                }
            }
        }

        [Test]
        public void Benchmark_Xxx_Ref()
        {
            int a = -1;
            int b = 0;

            for (int i = 0; i < 100000000; i++) {
                RawCopy.RefInt32(&b) = RawCopy.RefInt32(&a);
            }
        }

        [Test]
        public void Benchmark_Xxx_ReadWrite()
        {
            int a = -1;
            int b = 0;

            for (int i = 0; i < 100000000; i++) {
                RawCopy.WriteInt32(&b, RawCopy.ReadInt32(&a));
            }
        }

        [Test]
        public void Benchmark_Xxx_ReadWriteValue()
        {
            int a = -1;
            int b = 0;

            for (int i = 0; i < 100000000; i++) {
                RawCopy.WriteInt32(&b, a);
            }
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