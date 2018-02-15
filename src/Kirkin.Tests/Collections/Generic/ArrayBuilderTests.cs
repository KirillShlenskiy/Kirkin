using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Kirkin.Collections.Generic;

using NUnit.Framework;

namespace Kirkin.Tests.Collections.Generic
{
    public class ArrayBuilderTests
    {
        const int NumIterations = 10000000;
        static readonly IEnumerable<int> EnumerableItems = Enumerable.Range(0, NumIterations);
        static readonly int[] PreallocatedItems = EnumerableItems.ToArray();

        static ArrayBuilderTests()
        {
            new ArrayBuilder<int>().Add(1);
            new ArrayBuilder<int>(1).FastAdd(1);
            new PooledArrayBuilder<int>().Add(1);
            new PooledArrayBuilder<int>(1).FastAdd(1);
            new List<int>().Add(1);
            ImmutableArray.Create(1);
        }

        [Test]
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

        [Test]
        public void AddManyBenchmarkArrayBuilder()
        {
            var builder = new ArrayBuilder<int>();

            for (int i = 0; i < NumIterations; i++)
            {
                builder.Add(i);
            }

            builder.ToArray();
        }

        [Test]
        public void AddManyBenchmarkArrayBuilderPooled()
        {
            var builder = new PooledArrayBuilder<int>();

            for (int i = 0; i < NumIterations; i++)
            {
                builder.Add(i);
            }

            builder.ToArray();
        }

        [Test]
        public void AddManyBenchmarkArrayBuilderWithCapacity()
        {
            var builder = new ArrayBuilder<int>(NumIterations);

            for (int i = 0; i < NumIterations; i++)
            {
                builder.FastAdd(i);
            }

            builder.ToArray();
        }

        [Test]
        public void AddManyBenchmarkArrayBuilderWithCapacityPooled()
        {
            var builder = new PooledArrayBuilder<int>(NumIterations);

            for (int i = 0; i < NumIterations; i++)
            {
                builder.FastAdd(i);
            }

            builder.ToArray();
        }

        [Test]
        public void AddManyBenchmarkImmutableBuilder()
        {
            var builder = ImmutableArray.CreateBuilder<int>(NumIterations);

            for (int i = 0; i < NumIterations; i++)
            {
                builder.Add(i);
            }

            builder.ToImmutable();
        }

        [Test]
        public void AddManyBenchmarkLinqToArray()
        {
            EnumerableItems.ToArray();
        }

        [Test]
        public void AddManyBenchmarkLinqToArrayPreallocated()
        {
            PreallocatedItems.ToArray();
        }

        [Test]
        public void AddManyBenchmarkLinqToImmutableArray()
        {
            EnumerableItems.ToImmutableArray();
        }

        [Test]
        public void AddManyBenchmarkLinqToImmutableArrayPreallocated()
        {
            // Perf: ToImmutableArray is optimized for cases
            // where collection count can be divined, so this
            // method is expected to perform much better than
            // its IEnumerable equivalent in some scenarios.
            PreallocatedItems.ToImmutableArray();
        }

        [Test]
        public void AddManyBenchmarkLinqToList()
        {
            EnumerableItems.ToList();
        }

        [Test]
        public void AddManyBenchmarkLinqToListPreallocated()
        {
            PreallocatedItems.ToList();
        }

        [Test]
        public void AddManyBenchmarkLinqViaBuilderAdd()
        {
            var builder = new ArrayBuilder<int>();

            foreach (var i in PreallocatedItems)
            {
                builder.Add(i);
            }

            builder.ToArray();
        }

        [Test]
        public void AddManyBenchmarkLinqViaBuilderAddPooled()
        {
            var builder = new PooledArrayBuilder<int>();

            foreach (var i in PreallocatedItems)
            {
                builder.Add(i);
            }

            builder.ToArray();
        }

        [Test]
        public void AddManyBenchmarkLinqViaBuilderAddWithCapacity()
        {
            var builder = new ArrayBuilder<int>(NumIterations);

            foreach (var i in PreallocatedItems)
            {
                builder.FastAdd(i);
            }

            builder.ToArray();
        }

        [Test]
        public void AddManyBenchmarkLinqViaBuilderAddWithCapacityPooled()
        {
            var builder = new PooledArrayBuilder<int>(NumIterations);

            foreach (var i in PreallocatedItems)
            {
                builder.FastAdd(i);
            }

            builder.ToArray();
        }

        [Test]
        public void AddManyBenchmarkList()
        {
            var builder = new List<int>();

            for (int i = 0; i < NumIterations; i++)
            {
                builder.Add(i);
            }

            builder.ToArray();
        }

        [Test]
        public void AddManyBenchmarkListWithCapacity()
        {
            var builder = new List<int>(NumIterations);

            for (int i = 0; i < NumIterations; i++)
            {
                builder.Add(i);
            }

            builder.ToArray();
        }

        [Test]
        public void AddOneBenchmarkArray()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new int[1];

                builder[0] = i;
            }
        }

        [Test]
        public void AddOneBenchmarkArrayBuilder()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new ArrayBuilder<int>();

                builder.Add(i);
            }
        }

        [Test]
        public void AddOneBenchmarkArrayBuilderPooled()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new PooledArrayBuilder<int>();

                builder.Add(i);
            }
        }

        [Test]
        public void AddOneBenchmarkArrayBuilderWithCapacity()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new ArrayBuilder<int>(1);

                builder.FastAdd(i);
            }
        }

        [Test]
        public void AddOneBenchmarkArrayBuilderWithCapacityPooled()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new PooledArrayBuilder<int>(1);

                builder.FastAdd(i);
            }
        }

        [Test]
        public void AddOneBenchmarkList()
        {
            for (int i = 0; i < NumIterations; i++)
            {
                var builder = new List<int>();

                builder.Add(i);
            }
        }

        [Test]
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