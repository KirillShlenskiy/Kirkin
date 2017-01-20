using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Kirkin.Collections.Generic;

using NUnit.Framework;

namespace Kirkin.Tests.Collections.Generic
{
    public class VectorTests
    {
        [Test]
        public void IterationPerfArray()
        {
            var items = new[] { 1, 2, 3 };

            for (int i = 0; i < 10000000; i++)
            {
                foreach (int item in items)
                {
                }
            }
        }

        [Test]
        public void IterationPerfArraySegment()
        {
            var items = new ArraySegment<int>(new[] { 1, 2, 3 });

            for (int i = 0; i < 10000000; i++)
            {
                foreach (int item in items)
                {
                }
            }
        }

        [Test]
        public void IterationPerfArraySlice()
        {
            var items = new ArraySlice<int>(new[] { 1, 2, 3 }, 0, 3);

            for (int i = 0; i < 10000000; i++)
            {
                foreach (int item in items)
                {
                }
            }
        }

        [Test]
        public void IterationPerfArrayViaIndexer()
        {
            var items = new[] { 1, 2, 3 };

            for (int i = 0; i < 10000000; i++)
            {
                for (int j = 0; j < items.Length; j++)
                {
                    int item = items[j];
                }
            }
        }

        [Test]
        public void IterationPerfVector()
        {
            var items = Vector.Create(1, 2, 3);

            for (int i = 0; i < 10000000; i++)
            {
                foreach (int item in items)
                {
                }
            }
        }

        [Test]
        public void IterationPerfVectorViaIndexer()
        {
            var items = Vector.Create(1, 2, 3);

            for (int i = 0; i < 10000000; i++)
            {
                for (int j = 0; j < items.Length; j++)
                {
                    int item = items[j];
                }
            }
        }

        [Test]
        public void IterationPerfVectorViaInterface()
        {
            IEnumerable<int> items = Vector.Create(1, 2, 3);

            for (int i = 0; i < 10000000; i++)
            {
                foreach (int item in items)
                {
                }
            }
        }

        [Test]
        public void IterationPerfGenericList()
        {
            var items = new List<int> { 1, 2, 3 };

            for (int i = 0; i < 10000000; i++)
            {
                foreach (int item in items) // Still fast because struct enumerator.
                {
                }
            }
        }

        [Test]
        public void IterationPerfGenericListViaIndexer()
        {
            var items = new List<int> { 1, 2, 3 };

            for (int i = 0; i < 10000000; i++)
            {
                for (int j = 0; j < items.Count; j++)
                {
                    int item = items[j];
                }
            }
        }

        static readonly int[] Integers = new int[1000];

        [Test]
        public void ToListPerf()
        {
            for (int i = 0; i < 10000; i++) {
                Integers.ToList();
            }
        }

        [Test]
        public void ToArrayPerf()
        {
            for (int i = 0; i < 10000; i++) {
                Integers.ToArray();
            }
        }

        [Test]
        public void ToImmutableArrayPerf()
        {
            for (int i = 0; i < 10000; i++) {
                Integers.ToImmutableArray();
            }
        }

        [Test]
        public void ToVectorPerf()
        {
            for (int i = 0; i < 10000; i++) {
                Integers.ToVector();
            }
        }

        [Test]
        public void ToVectorEnumerablePerf()
        {
            for (int i = 0; i < 10000; i++) {
                Enumerable.Range(0, Integers.Length).ToVector();
            }
        }

        [Test]
        public void Empty()
        {
            var vector = Vector<int>.Empty;

            Assert.True(vector == Vector.Create<int>());
            Assert.True(vector != null);
        }

        [Test]
        public void IsDefaultOrEmpty()
        {
            Assert.True(default(Vector<int>).IsDefaultOrEmpty);
            Assert.True(Vector<int>.Empty.IsDefaultOrEmpty);
            Assert.False(Vector.Create(1).IsDefaultOrEmpty);
        }

        [Test]
        public void EqualityCheckActsLikeRef()
        {
            var vec1 = Vector.Create(1, 2, 3);
            var vec2 = Vector.Create(1, 2, 3);

            Assert.False(vec1 == vec2);
        }

        [Test]
        public void UninitializedEquatesToNull()
        {
            var vec = default(Vector<int>);

            Assert.True(vec == null);
        }

        [Test]
        public void GetEnumeratorThrowsIfDefault()
        {
            var vec = default(Vector<int>);

            Assert.True(vec.IsDefault);
            Assert.Throws<NullReferenceException>(() => vec.GetEnumerator());
            Assert.Throws<InvalidOperationException>(() => ((IEnumerable<int>)vec).GetEnumerator());
        }

        [Test]
        public void LinqMethodsThrowNullRefOnDefault()
        {
            var vec = default(Vector<int>);
            
            // LINQ methods implemented on Vector will throw a NullReferenceException.
            Assert.Throws<NullReferenceException>(() => vec.Any());
            Assert.Throws<NullReferenceException>(() => vec.Any(i => i == 0));
            Assert.Throws<NullReferenceException>(() => vec.All(i => i == 0));
            Assert.Throws<NullReferenceException>(() => vec.ElementAt(0));
            Assert.Throws<NullReferenceException>(() => vec.ElementAtOrDefault(0));
            Assert.Throws<NullReferenceException>(() => vec.First());
            Assert.Throws<NullReferenceException>(() => vec.First(i => i == 0));
            Assert.Throws<NullReferenceException>(() => vec.FirstOrDefault());
            Assert.Throws<NullReferenceException>(() => vec.FirstOrDefault(i => i == 0));
            Assert.Throws<NullReferenceException>(() => vec.Last());
            Assert.Throws<NullReferenceException>(() => vec.Last(i => i == 0));
            Assert.Throws<NullReferenceException>(() => vec.LastOrDefault());
            Assert.Throws<NullReferenceException>(() => vec.LastOrDefault(i => i == 0));
            Assert.Throws<NullReferenceException>(() => vec.Select(i => i));
            Assert.Throws<NullReferenceException>(() => vec.SelectMany(i => Vector.Create(i), (i, col) => i));
            Assert.Throws<NullReferenceException>(() => vec.SequenceEqual(Vector<int>.Empty));
            Assert.Throws<NullReferenceException>(() => vec.SequenceEqual(Enumerable.Empty<int>()));
            Assert.Throws<NullReferenceException>(() => vec.SequenceEqual(Vector<int>.Empty, (i1, i2) => i1 == i2));
            Assert.Throws<NullReferenceException>(() => vec.Single());
            Assert.Throws<NullReferenceException>(() => vec.Single(i => i == 0));
            Assert.Throws<NullReferenceException>(() => vec.SingleOrDefault());
            Assert.Throws<NullReferenceException>(() => vec.SingleOrDefault(i => i == 0));
            Assert.Throws<NullReferenceException>(() => vec.ToArray());
            Assert.Throws<NullReferenceException>(() => vec.Where(i => i == 0));

            // LINQ methods from System.Collections.Enumerable will throw an InvalidOperationException.
            Assert.Throws<InvalidOperationException>(() => vec.Count());
            Assert.Throws<InvalidOperationException>(() => vec.Count(i => i == 0));
            Assert.Throws<InvalidOperationException>(() => vec.ToList());
        }

        [Test]
        public void ArrayCopyPerf()
        {
            for (int i = 0; i < 1000000; i++)
            {
                int[] copy = new int[Integers.Length];

                Array.Copy(Integers, 0, copy, 0, Integers.Length);
            }
        }

        [Test]
        public void ArrayClonePerf()
        {
            for (int i = 0; i < 1000000; i++) {
                int[] copy = (int[])Integers.Clone();
            }
        }

        [Test]
        public void InterlockedBasic()
        {
            Vector<int> vec = Vector.Create(1);
            Vector<int> copy = vec;

            Assert.AreEqual(vec, copy);
            Assert.AreEqual(new[] { 1, 2 }, Vector.InterlockedAdd(ref vec, 2));
            Assert.AreNotEqual(vec, copy);
        }
    }
}