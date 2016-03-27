using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;

using Kirkin.Collections.Generic;
using Kirkin.Linq.Expressions;
using Kirkin.Mapping;

using Xunit;

namespace Kirkin.Tests.Experimental
{
    public class ExperimentalTests
    {
        public ExperimentalTests()
        {
            AutoMapper.Mapper.CreateMap<DatabaseDummy, Dummy>().ForMember(d => d.ID, c => c.NullSubstitute(0));
            AutoMapper.Mapper.AssertConfigurationIsValid();
        }

        [Fact]
        public void InterlockedWrap()
        {
            var i = int.MaxValue;

            checked {
                Assert.Equal(int.MinValue, Interlocked.Increment(ref i));
            }

            Assert.True((uint)int.MaxValue < unchecked((uint)int.MinValue));
        }

        [Fact]
        public void StructByValTest()
        {
            var builder = new Array<int>.Builder(1);

            builder.Add(1);

            for (int i = 0; i < 100000000; i++)
            {
                MaterialiseVal(builder);
            }
        }

        [Fact]
        public void StructByRefTest()
        {
            var builder = new Array<int>.Builder(1);

            builder.Add(1);

            for (int i = 0; i < 100000000; i++)
            {
                MaterialiseRef(ref builder);
            }
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        private static T[] MaterialiseVal<T>(Array<T>.Builder builder)
        {
            return builder.ToArray();
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        private static T[] MaterialiseRef<T>(ref Array<T>.Builder builder)
        {
            return builder.ToArray();
        }

        [Fact]
        public void AutomapperTest()
        {
            var dummy = new Dummy { ID = 1 };
            var databaseDummy = new DatabaseDummy { ID = null };

            for (var i = 0; i < 100000; i++)
            {
                AutoMapper.Mapper.Map(databaseDummy, dummy);
            }

            Assert.Equal(0, dummy.ID);
        }

        [Fact]
        public void TypeMappingTest()
        {
            var mapping = PropertyList<Dummy>.Default;
            var d1 = new Dummy { ID = 1 };
            var d2 = new Dummy { ID = 2 };

            for (var i = 0; i < 100000; i++)
            {
                Mapper.Map(d1, d2);
            }

            Assert.Equal(1, d2.ID);
        }

        [Fact]
        public void CartesianProductWithAggregate()
        {
            var r1 = Mash(
                new [] {
                    new [] { "red", "blue", "orange" },
                    new [] { "5 inche", "8 inch" },
                    new [] { "x", "y" },
                    new [] { "1", "2", "3" }
                },
                (x, y) => x + " " + y);

            var r2 = Mash(
                new [] {
                    new [] { 1, 2, 3 },
                    new [] { 2, 4, 8 },
                    new [] { 3, 6, 9 }
                },
                (x, y) => x * y);

            Debug.Print("Done.");
        }

        IEnumerable<T> Mash<T>(IEnumerable<IEnumerable<T>> list, Func<T, T, T> aggregator)
        {
            return list
                .DefaultIfEmpty(Enumerable.Empty<T>()) // Prevent exception if list is empty.
                .Aggregate((xs, ys) => xs.SelectMany(x => ys.Select(y => aggregator(x, y))))
                .ToList();
        }

        class Dummy
        {
            public int ID { get; set; }
        }

        class DatabaseDummy
        {
            public int? ID { get; set; }
        }

        public int Aperture(int input)
        {
            var binaryString = Convert.ToString(input, 2);

            // The accumulator is an integer array maintaining
            // the count of '0's since the last seen '1'.
            // Whenever a '1' is encountered, a new count
            // of zero is added at the end of the array.
            // Whenever a '0' is encountered, the last
            // count is incremented by one.
            var segments = binaryString.Aggregate(
                new [] { 0 },
                (acc, c) =>
                    c == '0'
                    ? acc
                        .Take(acc.Length - 1)
                        .Concat(new [] { acc[acc.Length - 1] + 1 })
                        .ToArray()
                    : acc
                        .Concat(new [] { 0 })
                        .ToArray()
            );

            return segments
                // If last segment count is non-zero, it was not
                // closed with a '1' and we want to exclude it.
                .Take(segments.Length - 1)
                .Max();
        }

        [Fact]
        public void ApertureTest()
        {
            Assert.Equal(2, Aperture(9));   // 1001,       Segments: 0, 2, 0
            Assert.Equal(4, Aperture(529)); // 1000010001, Segments: 0, 4, 3, 0
            Assert.Equal(1, Aperture(20));  // 10100,      Segments: 0, 1, 2
            Assert.Equal(0, Aperture(15));  // 1111,       Segments: 0, 0, 0, 0
        }

        [Fact]
        public void NullDelegateCapture()
        {
            var anon = new { Collection = Enumerable.Range(0, 1) };

            anon = null;

            var lazy1 = new Lazy<int[]>(() => anon.Collection.ToArray());
            var lazy2 = Assert.Throws<NullReferenceException>(() => new Lazy<int[]>(anon.Collection.ToArray));
        }

        [Fact]
        public void SemaphoreSlimNonReentrant()
        {
            var semaphore = new SemaphoreSlim(1, 1);

            Assert.True(semaphore.Wait(0));
            Assert.False(semaphore.Wait(0));
        }

        /// <summary>
        /// This concept is taken from Roslyn's ByteArrayUnion.
        /// </summary>
        [Fact]
        public void StripPrivateFieldFromStruct()
        {
            // NonNullableString only contains one field of type System.String.
            StringUnion union = new StringUnion {
                NonNullableString = new NonNullableString("Zzz")
            };

            Assert.Equal("Zzz", union.String);

            union = new StringUnion { String = "Uuu" };

            Assert.Equal(new NonNullableString("Uuu"), union.NonNullableString);
        }

        [StructLayout(LayoutKind.Explicit)]
        struct StringUnion
        {
            [FieldOffset(0)]
            public string String;

            [FieldOffset(0)]
            public NonNullableString NonNullableString;
        }

        [Fact]
        public void PropertyInfoEq()
        {
            var p1 = typeof(Dummyz).GetProperties()[0];
            var p2 = typeof(Dummyz).GetProperty("ID");

            Assert.True(p1.Equals(p2));
            Assert.True(Equals(p1, p2));
            Assert.True(p1 == p2);
        }

        class Dummyz
        {
            public int ID { get; set; }
        }
    }

    internal static class Mutation
    {
        public static void Mutate<TObj, TProperty>(TObj obj, Expression<Func<TObj, TProperty>> propExpr, Action<TProperty> mutation)
            where TProperty : struct
        {
            var prop = ExpressionUtil.Property<TObj, TProperty>(propExpr);
        }
    }
}