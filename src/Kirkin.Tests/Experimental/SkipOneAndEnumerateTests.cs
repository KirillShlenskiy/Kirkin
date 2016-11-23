using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Kirkin.Tests.Experimental
{
    public class SkipOneAndEnumerateTests
    {
        const int Iterations = 100000;

        [Theory]
        [InlineData(4)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        public void ArrayCopy(int count) // 2nd place.
        {
            int[] array = CreateArray(count);

            for (int i = 0; i < Iterations; i++)
            {
                int[] skipResult = new int[array.Length - 1];

                for (int j = 0; j < skipResult.Length; j++) {
                    skipResult[j] = array[j + 1];
                }

                Enumerate(skipResult);
            }
        }

        [Theory]
        [InlineData(4)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        public void ArraySegment(int count) // Winner.
        {
            int[] array = CreateArray(count);

            for (int i = 0; i < Iterations; i++)
            {
                ArraySegment<int> skipResult = new ArraySegment<int>(array, 1, array.Length - 1);

                Enumerate(skipResult);
            }
        }

        [Theory]
        [InlineData(4)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        public void LinqSkip(int count) // 3rd (last) place.
        {
            int[] array = CreateArray(count);

            for (int i = 0; i < Iterations; i++)
            {
                IEnumerable<int> skipResult = array.Skip(1);

                Enumerate(skipResult);
            }
        }

        private static void Enumerate<T>(IEnumerable<T> enumerable)
        {
            foreach (T element in enumerable)
            {
            }
        }

        private static int[] CreateArray(int count)
        {
            int[] array = new int[count];

            for (int i = 0; i < count; i++) {
                array[i] = i;
            }

            return array;
        }
    }
}