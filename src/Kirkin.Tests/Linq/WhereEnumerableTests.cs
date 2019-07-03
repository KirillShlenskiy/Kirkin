using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Kirkin.Collections.Generic;
using Kirkin.Linq;

using NUnit.Framework;

namespace Kirkin.Tests.Linq
{
    public class WhereEnumerableTests
    {
        [Test]
        public void BasicCount()
        {
            int[] array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            Assert.AreEqual(5, array.Count(i => i % 2 == 0));
            Assert.AreEqual(5, array.Where(i => i % 2 == 0).Count());
        }

        [Test]
        public void ArrayCountBenchmarkArrayExtensions()
        {
            int[] array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int q = 0; q < 10000000; q++) {
                ArrayExtensions.Count(array, i => i % 2 == 0);
            }
        }

        [Test]
        public void ArrayCountBenchmarkWhereEnumerable()
        {
            int[] array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int q = 0; q < 10000000; q++) {
                ArrayWhereExtensions.Where(array, i => i % 2 == 0).Count();
            }
        }

        [Test]
        public void ArrayCountBenchmarkLinq()
        {
            int[] array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int q = 0; q < 10000000; q++) {
                Enumerable.Count(array, i => i % 2 == 0);
            }
        }

        [Test]
        public void ToArrayBenchmarkWhereEnumerable()
        {
            int[] array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int q = 0; q < 2000000; q++) {
                ArrayWhereExtensions.Where(array, i => i % 2 == 0).ToArray();
            }
        }

        [Test]
        public void ToArrayBenchmarkLinq()
        {
            int[] array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int q = 0; q < 2000000; q++) {
                Enumerable.Where(array, i => i % 2 == 0).ToArray();
            }
        }
    }

    internal static class ArrayWhereExtensions
    {
        public static ArrayWhereEnumerable<T> Where<T>(this T[] array, Func<T, bool> predicate) {
            return new ArrayWhereEnumerable<T>(array, predicate);
        }
    }

    public struct ArrayWhereEnumerable<T> : IEnumerable<T>
    {
        public T[] Array { get; }
        public Func<T, bool> Predicate { get; }

        public ArrayWhereEnumerable(T[] array, Func<T, bool> predicate)
        {
            Array = array;
            Predicate = predicate;
        }

        public bool Any()
        {
            return GetEnumerator().MoveNext();
        }

        public bool Any(Func<T, bool> predicate)
        {
            foreach (T item in this)
            {
                if (predicate(item)) {
                    return true;
                }
            }

            return false;
        }

        public int Count()
        {
            ArrayWhereEnumerator<T> enumerator = GetEnumerator();
            int count = 0;

            while (enumerator.MoveNext())
            {
                checked {
                    count++;
                }
            }

            return count;
        }

        public int Count(Func<T, bool> predicate)
        {
            int count = 0;

            foreach (T item in this)
            {
                if (predicate(item)) {
                    count++;
                }
            }

            return count;
        }

        public T[] ToArray()
        {
            ArrayBuilder<T> builder = new ArrayBuilder<T>();

            foreach (T item in this) {
                builder.Add(item);
            }

            return builder.ToArray();
        }

        public IEnumerable<T> AsEnumerable() => Enumerable.Where(Array, Predicate);
        public ArrayWhereEnumerator<T> GetEnumerator() => new ArrayWhereEnumerator<T>(Array, Predicate);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => AsEnumerable().GetEnumerator();
    }

    public struct ArrayWhereEnumerator<T>
    {
        public T[] Array { get; }
        public Func<T, bool> Predicate { get; }
        public T Current { get; }

        private int Index;

        public ArrayWhereEnumerator(T[] array, Func<T, bool> predicate)
        {
            Array = array;
            Predicate = predicate;
            Current = default;
            Index = -1;
        }

        public bool MoveNext()
        {
            while (true)
            {
                if (++Index == Array.Length) return false;
                if (Predicate(Array[Index])) return true;
            }
        }
    }
}