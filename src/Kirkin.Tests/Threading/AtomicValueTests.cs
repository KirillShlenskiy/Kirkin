using System.Threading;
using System.Threading.Tasks;

using Kirkin.Threading;

using NUnit.Framework;

namespace Kirkin.Tests.Threading
{
    public class AtomicValueTests
    {
        /// <summary>
        /// This is here to ensure our Version does not require overflow checks.
        /// </summary>
        [Test]
        public void OverflowOddness()
        {
            Assert.True(int.MaxValue % 2 != 0); // Odd.
            Assert.True(int.MinValue % 2 == 0); // Even.

            int i = int.MaxValue;
            Assert.True(i % 2 != 0); // Odd.
            Assert.True(Interlocked.Increment(ref i) % 2 == 0); // Negative even.
            Assert.True(Interlocked.Increment(ref i) % 2 != 0);

            i = -1;

            Assert.True(i % 2 != 0); // Odd.
            Assert.True(Interlocked.Increment(ref i) % 2 == 0); // Even.
            Assert.AreEqual(0, i);
        }

        // 96-bit struct ensures.
        // Non-atomic writes on x86 and x64.
        struct LargeStruct
        {
            public int A;
            public int B;
            public int C;
        }

        [Test]
        public void AtomicSucceeds()
        {
            Atomic<LargeStruct> atomic = new Atomic<LargeStruct>();

            Task loop = Task.Run(() =>
            {
                for (int i = 0; i < 10000000; i++) {
                    atomic.Value = new LargeStruct { A = i, B = i, C = i };
                }
            });

            while (!loop.IsCompleted)
            {
                LargeStruct value = atomic.Value;

                Assert.True(value.A == value.B && value.B == value.C);
            }
        }

        [Test]
        public void NonAtomicFails()
        {
            NonAtomic<LargeStruct> nonAtomic = new NonAtomic<LargeStruct>();

            Task loop = Task.Run(() =>
            {
                for (int i = 0; i < 10000000; i++) {
                    nonAtomic.Value = new LargeStruct { A = i, B = i, C = i };
                }
            });

            while (!loop.IsCompleted)
            {
                LargeStruct value = nonAtomic.Value;

                if (value.A != value.B || value.B != value.C) {
                    return; // Torn read.
                }
            }

            Assert.True(false, "Got to the end without a torn read.");
        }

        [Test]
        public void NonAtomicPerf()
        {
            NonAtomic<LargeStruct> nonAtomic = new NonAtomic<LargeStruct>();

            Task loop = Task.Run(() =>
            {
                for (int i = 0; i < 10000000; i++) {
                    nonAtomic.Value = new LargeStruct { A = i, B = i, C = i };
                }
            });

            while (!loop.IsCompleted)
            {
                LargeStruct value = nonAtomic.Value;

                if (value.A != value.B || value.B != value.C) {
                    // Torn read.
                }
            }
        }

        sealed class NonAtomic<T>
        {
            internal T Value;
        }
    }
}