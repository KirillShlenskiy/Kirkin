using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Kirkin.Collections.Generic;

using NUnit.Framework;

namespace Kirkin.Tests.Collections.Generic
{
    public class TinyBufferTests
    {
        [Test]
        public void BasicToArray()
        {
            var buffer = new TinyBuffer<int>();

            Assert.AreEqual(0, buffer.Count);

            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual(new[] { 1, 2, 3 }, buffer.ToArray());
        }

        const int ITERATIONS8 = 1000000;

        [Test]
        public void PerfArray1()
        {
            PerfArray(1);
        }

        [Test]
        public void PerfArray7()
        {
            PerfArray(7);
        }

        [Test]
        public void PerfArray8()
        {
            PerfArray(8);
        }

        [Test]
        public void PerfArray15()
        {
            PerfArray(15);
        }

        [Test]
        public void PerfArray100()
        {
            PerfArray(100);
        }

        void PerfArray(int count)
        {
            for (int i = 0; i < ITERATIONS8; i++)
            {
                int[] arr = new int[8];

                for (int j = 0; j < count; j++)
                {
                    if (j == arr.Length) {
                        Array.Resize(ref arr, arr.Length * 2);
                    }

                    arr[j] = j;
                }

                if (arr.Length != count) {
                    Array.Resize(ref arr, count);
                }
            }
        }

        [Test]
        public void PerfTinyBuffer1()
        {
            PerfTinyBuffer(1);
        }

        [Test]
        public void PerfTinyBuffer7()
        {
            PerfTinyBuffer(7);
        }

        [Test]
        public void PerfTinyBuffer8()
        {
            PerfTinyBuffer(8);
        }

        [Test]
        public void PerfTinyBuffer15()
        {
            PerfTinyBuffer(15);
        }

        [Test]
        public void PerfTinyBuffer100()
        {
            PerfTinyBuffer(100);
        }

        void PerfTinyBuffer(int count)
        {
            for (int i = 0; i < ITERATIONS8; i++)
            {
                var builder = new TinyBuffer<int>();

                for (int j = 0; j < count; j++) {
                    builder.Add(j);
                }

                builder.ToArray();
            }
        }

        [Test]
        public void PerfArrayBuilder1()
        {
            PerfArrayBuilder(1);
        }

        [Test]
        public void PerfArrayBuilder7()
        {
            PerfArrayBuilder(7);
        }

        [Test]
        public void PerfArrayBuilder8()
        {
            PerfArrayBuilder(8);
        }

        [Test]
        public void PerfArrayBuilder15()
        {
            PerfArrayBuilder(15);
        }

        [Test]
        public void PerfArrayBuilder100()
        {
            PerfArrayBuilder(100);
        }

        void PerfArrayBuilder(int count)
        {
            for (int i = 0; i < ITERATIONS8; i++)
            {
                var builder = new ArrayBuilder<int>();

                for (int j = 0; j < count; j++) {
                    builder.Add(j);
                }

                builder.ToArray();
            }
        }

        [Test]
        public void PerfPooledArrayBuilder1()
        {
            PerfPooledArrayBuilder(1);
        }

        [Test]
        public void PerfPooledArrayBuilder7()
        {
            PerfPooledArrayBuilder(7);
        }

        [Test]
        public void PerfPooledArrayBuilder8()
        {
            PerfPooledArrayBuilder(8);
        }

        [Test]
        public void PerfPooledArrayBuilder15()
        {
            PerfPooledArrayBuilder(15);
        }

        [Test]
        public void PerfPooledArrayBuilder100()
        {
            PerfPooledArrayBuilder(100);
        }

        void PerfPooledArrayBuilder(int count)
        {
            for (int i = 0; i < ITERATIONS8; i++)
            {
                var builder = new PooledArrayBuilder<int>();

                for (int j = 0; j < count; j++) {
                    builder.Add(j);
                }

                builder.ToArray();
            }
        }

        [Test]
        public void PerfImmutableBuilder1()
        {
            PerfImmutableBuilder(1);
        }

        [Test]
        public void PerfImmutableBuilder7()
        {
            PerfImmutableBuilder(7);
        }

        [Test]
        public void PerfImmutableBuilder8()
        {
            PerfImmutableBuilder(8);
        }

        [Test]
        public void PerfImmutableBuilder15()
        {
            PerfImmutableBuilder(15);
        }

        [Test]
        public void PerfImmutableBuilder100()
        {
            PerfImmutableBuilder(100);
        }

        void PerfImmutableBuilder(int count)
        {
            for (int i = 0; i < ITERATIONS8; i++)
            {
                var builder = ImmutableArray.CreateBuilder<int>();

                for (int j = 0; j < count; j++) {
                    builder.Add(j);
                }

                builder.ToImmutable();
            }
        }

        [Test]
        public void PerfList1()
        {
            PerfList(1);
        }

        [Test]
        public void PerfList7()
        {
            PerfList(7);
        }

        [Test]
        public void PerfList8()
        {
            PerfList(8);
        }

        [Test]
        public void PerfList15()
        {
            PerfList(15);
        }

        [Test]
        public void PerfList100()
        {
            PerfList(100);
        }

        void PerfList(int count)
        {
            for (int i = 0; i < ITERATIONS8; i++)
            {
                var builder = new List<int>();

                for (int j = 0; j < count; j++) {
                    builder.Add(j);
                }

                builder.ToArray();
            }
        }
    }
}