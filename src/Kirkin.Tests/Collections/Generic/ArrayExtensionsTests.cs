using System.Linq;

using Xunit;

namespace Kirkin.Tests.Collections.Generic
{
    public class ArrayExtensionsTests
    {
        static readonly int[] Integers = new int[1000];

        [Fact]
        public void AggregatePerfSystemLinq()
        {
            int sum = 0;

            for (int i = 0; i < 100000; i++)
            {
                sum = Enumerable.Aggregate(Integers, 0, (acc, seed) => acc + seed);
            }
        }

        [Fact]
        public void AggregatePerfArrayExtensions()
        {
            int sum = 0;

            for (int i = 0; i < 100000; i++)
            {
                sum = Integers.Aggregate(0, (acc, seed) => acc + seed);
            }
        }

        [Fact]
        public void FirstOrDefaultPerfSystemLinq()
        {
            for (int i = 0; i < 10000000; i++)
            {
                var z = Enumerable.FirstOrDefault(Integers);
            }
        }

        [Fact]
        public void FirstOrDefaultPerfArrayExtensions()
        {
            for (int i = 0; i < 10000000; i++)
            {
                var z = Integers.FirstOrDefault();
            }
        }

        [Fact]
        public void FirstOrDefaultWithPredicatePerfSystemLinq()
        {
            for (int i = 0; i < 100000; i++)
            {
                var z = Enumerable.FirstOrDefault(Integers, o => o == 2);
            }
        }

        [Fact]
        public void FirstOrDefaultWithPredicatePerfArrayExtensions()
        {
            for (int i = 0; i < 100000; i++)
            {
                var z = Integers.FirstOrDefault(o => o == 2);
            }
        }

        // SelectMany was found to be faster in System.Linq.
        //[Fact]
        //public void SelectManyPerfSystemLinq()
        //{
        //    for (int i = 0; i < 10; i++)
        //    {
        //        var query =
        //            from a in Integers
        //            from b in Integers
        //            select new KeyValuePair<int, int>(a, b);

        //        query.ToArray();
        //    }
        //}

        [Fact]
        public void ToArrayPerfSystemLinq()
        {
            for (int i = 0; i < 100000; i++) {
                Enumerable.ToArray(Integers);
            }
        }

        [Fact]
        public void ToArrayPerfArrayExtensions()
        {
            for (int i = 0; i < 100000; i++) {
                Integers.ToArray();
            }
        }
    }
}