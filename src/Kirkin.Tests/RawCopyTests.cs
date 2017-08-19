using System;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public unsafe class RawCopyTests
    {
        [Test]
        public void RawCopyInt32()
        {
            int a = -1;
            int b = 0;

            RawCopy.CopyBytes(&a, &b, sizeof(int));

            Assert.AreEqual(-1, b);
        }

        [Test]
        public void RawCopyBool()
        {
            bool value = true;
            int result = 0;

            RawCopy.CopyBytes(&value, 0, &result, 0, sizeof(bool));

            Assert.AreEqual(1, result);

            RawCopy.CopyBytes(&value, 0, &result, 1, sizeof(bool));

            Assert.AreEqual(257, result);
        }

        [Test]
        public void ReadWriteInt8()
        {
            Block8 value = new Block8();

            RawCopy.WriteInt32(&value, 1);

            Assert.AreEqual(1, RawCopy.ReadInt32(&value));
        }

        [Test]
        public void ReadWriteInt32()
        {
            Block32 value = new Block32();

            Assert.AreEqual(0, RawCopy.ReadInt32(&value));

            RawCopy.WriteBytes(&value, new byte[] { 255, 255, 255, 255 });

            Assert.AreEqual(-1, RawCopy.ReadInt32(&value));
        }

        [Test]
        public void ReadWriteInt64()
        {
            Block64 value = new Block64();

            Assert.AreEqual(0, RawCopy.ReadInt64(&value));

            RawCopy.WriteInt64(&value, long.MaxValue);

            Assert.AreEqual(-1, RawCopy.ReadInt32(&value));
            Assert.AreEqual(int.MaxValue, RawCopy.ReadInt32(&value, 4));

            RawCopy.WriteInt32(&value, 4, -1);

            Assert.AreEqual(-1, RawCopy.ReadInt64(&value));
        }

        [Test]
        public void ReadWriteBytes()
        {
            byte[] bytes;
            int value = 0;

            bytes = RawCopy.ReadBytes(&value, sizeof(int));

            Assert.AreEqual(0, bytes[0]);
            Assert.AreEqual(0, bytes[1]);
            Assert.AreEqual(0, bytes[2]);
            Assert.AreEqual(0, bytes[3]);

            RawCopy.WriteBytes(&value, new byte[] { 255, 255, 255, 255 });

            Assert.AreEqual(-1, RawCopy.ReadInt32(&value));

            bytes = RawCopy.ReadBytes(&value, sizeof(int));

            Assert.AreEqual(255, bytes[0]);
            Assert.AreEqual(255, bytes[1]);
            Assert.AreEqual(255, bytes[2]);
            Assert.AreEqual(255, bytes[3]);
        }

        [Test]
        public void RawCopyBoxedInt32()
        {
            object a = -1;
            object b = 0;

            GCHandle handleA = GCHandle.Alloc(a, GCHandleType.Pinned);
            GCHandle handleB = GCHandle.Alloc(b, GCHandleType.Pinned);

            RawCopy.CopyBytes((IntPtr*)handleA.AddrOfPinnedObject(), (IntPtr*)handleB.AddrOfPinnedObject(), sizeof(int));

            Assert.AreEqual(-1, b);

            handleA.Free();
            handleB.Free();
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