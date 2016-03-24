using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Kirkin.Collections.Generic;

using Xunit;

namespace Kirkin.Tests.Collections.Generic
{
    public class ArrayBuilderTests
    {
        const int NumIterations = 10000000;
        static readonly IEnumerable<int> EnumerableItems = Enumerable.Range(0, NumIterations);
        static readonly int[] PreallocatedItems = EnumerableItems.ToArray();

        static ArrayBuilderTests()
        {
            new Array<int>.Builder().Add(1);
            new Array<int>.Builder(1).UnsafeAdd(1);
            new List<int>().Add(1);
            ImmutableArray.Create(1);
        }

        [Fact]
        public void AddManyBenchmarkArray()
        {
            var builder = new int[NumIterations];
            int count = 0;

            for (int i = 0; i < NumIterations; i++)
            {
                builder[count] = i;
                count++;
            }

            Array.Resize(ref builder, count);
        }

        [Fact]
        public void AddManyBenchmarkArrayBuilder()
        {
            var builder = new Array<int>.Builder();

            for (int i = 0; i < NumIterations; i++)
            {
                builder.Add(i);
            }

            builder.ToArray();
        }

        [Fact]
        public void AddManyBenchmarkArrayBuilderWithCapacity()
        {
            var builder = new Array<int>.Builder(NumIterations);

            for (int i = 0; i < NumIterations; i++)
            {
                builder.UnsafeAdd(i);
            }

            builder.ToArray();
        }

        [Fact]
        public void AddManyBenchmarkImmutableBuilder()
        {
            var builder = ImmutableArray.CreateBuilder<int>(NumIterations);

            for (int i = 0; i < NumIterations; i++)
            {
                builder.Add(i);
            }

            builder.ToImmutable();
        }

        [Fact]
        public void AddManyBenchmarkLinqToArray()
        {
            EnumerableItems.ToArray();
        }

        [Fact]
        public void AddManyBenchmarkLinqToArrayPreallocated()
        {
            PreallocatedItems.ToArray();
        }

        [Fact]
        public void AddManyBenchmarkLinqToImmutableArray()
        {
            EnumerableItems.ToImmutableArray();
        }

        [Fact]
        public void AddManyBenchmarkLinqToImmutableArrayPreallocated()
        {
            // Perf: ToImmutableArray is optimised for cases
            // where collection count can be divined, so this
            // method is expected to perform much better than
            // its IEnumerable equivalent in some scenarios.
            PreallocatedItems.ToImmutableArray();
        }

        [Fact]
        public void AddManyBenchmarkLinqToList()
        {
            EnumerableItems.ToList();
        }

        [Fact]
        public void AddManyBenchmarkLinqToListPreallocated()
        {
            PreallocatedItems.ToList();
        }

        [Fact]
        public void AddManyBenchmarkLinqViaBuilderAdd()
        {
            var builder = new Array<int>.Builder();

            foreach (var i in PreallocatedItems)
            {
                builder.Add(i);
            }

            builder.ToArray();
        }

        [Fact]
        public void AddManyBenchmarkLinqViaBuilderAddWithCapacity()
        {
            var builder = new Array<int>.Builder(NumIterations);

            foreach (var i in PreallocatedItems)
            {
                builder.UnsafeAdd(i);
            }

            builder.ToArray();
        }

        [Fact]
        public void AddManyBenchmarkList()
        {
            var builder = new List<int>();

            for (int i = 0; i < NumIterations; i++)
            {
                builder.Add(i);
            }

            builder.ToArray();
        }

        [Fact]
        public void AddManyBenchmarkListWithCapacity()
        {
            var builder = new List<int>(NumIterations);

            for (int i = 0; i < NumIterations; i++)
            {
                builder.Add(i);
            }

            builder.ToArray();
        }

        [Fact]
        public void AddOneBenchmarkArray()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new int[1];

                builder[0] = i;
            }
        }

        [Fact]
        public void AddOneBenchmarkArrayBuilder()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new Array<int>.Builder();

                builder.Add(i);
            }
        }

        [Fact]
        public void AddOneBenchmarkArrayBuilderWithCapacity()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new Array<int>.Builder(1);

                builder.UnsafeAdd(i);
            }
        }

        [Fact]
        public void AddOneBenchmarkList()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new List<int>();

                builder.Add(i);
            }
        }

        [Fact]
        public void AddOneBenchmarkListWithCapacity()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new List<int>(1);

                builder.Add(i);
            }
        }
    }
}