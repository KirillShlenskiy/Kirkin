using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Kirkin.Collections.Generic;

using Xunit;

namespace Kirkin.Tests.Collections.Generic
{
    public class TinyBufferTests
    {
        [Fact]
        public void BasicToArray()
        {
            var buffer = new TinyBuffer<int>();

            Assert.Equal(0, buffer.Count);

            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            Assert.Equal(3, buffer.Count);
            Assert.Equal(new[] { 1, 2, 3 }, buffer.ToArray());
        }

        const int ITERATIONS8 = 1000000;

        [Fact]
        public void PerfArray1()
        {
            PerfArray(1);
        }

        [Fact]
        public void PerfArray7()
        {
            PerfArray(7);
        }

        [Fact]
        public void PerfArray8()
        {
            PerfArray(8);
        }

        [Fact]
        public void PerfArray15()
        {
            PerfArray(15);
        }

        [Fact]
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

        [Fact]
        public void PerfTinyBuffer1()
        {
            PerfTinyBuffer(1);
        }

        [Fact]
        public void PerfTinyBuffer7()
        {
            PerfTinyBuffer(7);
        }

        [Fact]
        public void PerfTinyBuffer8()
        {
            PerfTinyBuffer(8);
        }

        [Fact]
        public void PerfTinyBuffer15()
        {
            PerfTinyBuffer(15);
        }

        [Fact]
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

        [Fact]
        public void PerfArrayBuilder1()
        {
            PerfArrayBuilder(1);
        }

        [Fact]
        public void PerfArrayBuilder7()
        {
            PerfArrayBuilder(7);
        }

        [Fact]
        public void PerfArrayBuilder8()
        {
            PerfArrayBuilder(8);
        }

        [Fact]
        public void PerfArrayBuilder15()
        {
            PerfArrayBuilder(15);
        }

        [Fact]
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

        [Fact]
        public void PerfPooledArrayBuilder1()
        {
            PerfPooledArrayBuilder(1);
        }

        [Fact]
        public void PerfPooledArrayBuilder7()
        {
            PerfPooledArrayBuilder(7);
        }

        [Fact]
        public void PerfPooledArrayBuilder8()
        {
            PerfPooledArrayBuilder(8);
        }

        [Fact]
        public void PerfPooledArrayBuilder15()
        {
            PerfPooledArrayBuilder(15);
        }

        [Fact]
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

        [Fact]
        public void PerfImmutableBuilder1()
        {
            PerfImmutableBuilder(1);
        }

        [Fact]
        public void PerfImmutableBuilder7()
        {
            PerfImmutableBuilder(7);
        }

        [Fact]
        public void PerfImmutableBuilder8()
        {
            PerfImmutableBuilder(8);
        }

        [Fact]
        public void PerfImmutableBuilder15()
        {
            PerfImmutableBuilder(15);
        }

        [Fact]
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

        [Fact]
        public void PerfList1()
        {
            PerfList(1);
        }

        [Fact]
        public void PerfList7()
        {
            PerfList(7);
        }

        [Fact]
        public void PerfList8()
        {
            PerfList(8);
        }

        [Fact]
        public void PerfList15()
        {
            PerfList(15);
        }

        [Fact]
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