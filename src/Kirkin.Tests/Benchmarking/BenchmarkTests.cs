using System.Collections.Immutable;
using System.Linq;

using Kirkin.Collections.Generic;

using BenchmarkDotNet.Attributes;

using Xunit;
using Xunit.Abstractions;

namespace Kirkin.Tests.Benchmarking
{
    public class BenchmarkTests
    {
        private readonly ITestOutputHelper Output;

        public BenchmarkTests(ITestOutputHelper output)
        {
            Output = output;
        }

        //[Fact]
        public void Collections()
        {
            string report = Benchmarks.Run<CollectionPerf>();

            Output.WriteLine(report);
        }

        public class CollectionPerf
        {
            private int[] Integers;

            [Setup]
            public void SetUp()
            {
                Integers = new int[100000];
            }

            [Benchmark]
            public void ToArray()
            {
                Integers.ToArray();
            }

            [Benchmark]
            public void ToList()
            {
                Integers.ToList();
            }

            [Benchmark]
            public void ToImmutableArray()
            {
                Integers.ToImmutableArray();
            }

            [Benchmark]
            public void ToVector()
            {
                Integers.ToVector();
            }
        }
    }
}