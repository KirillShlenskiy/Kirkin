using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Kirkin.Collections.Generic;
using Kirkin.Collections.Generic.Enumerators;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Factory and extension methods for working with <see cref="Vector{T}"/>.
    /// </summary>
    /// <remarks>
    /// This is not original work.
    /// Large chunks are borrowed from System.Collections.Immutable.ImmutableArray{T}.
    /// </remarks>
    public static class Vector
    {
        #region Factory methods

        /// <summary>
        /// Returns an empty <see cref="Vector{T}"/>.
        /// </summary>
        public static Vector<T> Create<T>()
        {
            return Vector<T>.Empty;
        }

        /// <summary>
        /// Returns a <see cref="Vector{T}"/> with a single item.
        /// </summary>
        public static Vector<T> Create<T>(T item)
        {
            return new Vector<T>(new T[] { item });
        }

        /// <summary>
        /// Returns a <see cref="Vector{T}"/> with two items.
        /// </summary>
        public static Vector<T> Create<T>(T item1, T item2)
        {
            return new Vector<T>(new T[] { item1, item2 });
        }

        /// <summary>
        /// Returns a <see cref="Vector{T}"/> with three items.
        /// </summary>
        public static Vector<T> Create<T>(T item1, T item2, T item3)
        {
            return new Vector<T>(new T[] { item1, item2, item3 });
        }

        /// <summary>
        /// Returns a <see cref="Vector{T}"/> with four items.
        /// </summary>
        public static Vector<T> Create<T>(T item1, T item2, T item3, T item4)
        {
            return new Vector<T>(new T[] { item1, item2, item3, item4 });
        }

        /// <summary>
        /// Returns a <see cref="Vector{T}"/> with the given items.
        /// </summary>
        public static Vector<T> Create<T>(params T[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            T[] defensiveCopy = new T[items.Length];

            Array.Copy(items, 0, defensiveCopy, 0, items.Length);

            return new Vector<T>(defensiveCopy);
        }

        /// <summary>
        /// Returns a <see cref="Vector{T}"/> with the given items.
        /// </summary>
        public static Vector<T> CreateRange<T>(IEnumerable<T> items)
        {
            // Arg null validation performed by ToArray.
            return new Vector<T>(items.ToArray());
        }

        #endregion

        #region Extension methods

        /// <summary>
        /// Copies the elements of this sequence to a new <see cref="Vector{T}"/>.
        /// </summary>
        public static Vector<T> ToVector<T>(this IEnumerable<T> items)
        {
            if (items is Vector<T>) return (Vector<T>)items;

            return CreateRange(items);
        }

        #endregion

        /// <summary>
        /// Atomically adds an item to the given <see cref="Vector{T}"/>,
        /// stores it at the given location and returns the result.
        /// </summary>
        public static Vector<T> InterlockedAdd<T>(ref Vector<T> vector, T item)
        {
            T[] current = vector.array, start, desired;

            do
            {
                start = current;
                desired = new T[current.Length + 1];

                Array.Copy(current, 0, desired, 0, current.Length);

                desired[current.Length] = item;
                current = Interlocked.CompareExchange(ref vector.array, desired, start);
            }
            while (start != current);

            return new Vector<T>(desired);
        }
    }

    /// <summary>
    /// Immutable lightweight collection type which allows accessing
    /// its elements with an index and provides a struct enumerator.
    /// </summary>
    /// <remarks>
    /// Portable alternative to System.Collections.Immutable.ImmutableArray{T}.
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct Vector<T>
        : IList<T>, IEquatable<Vector<T>>
#if !NET_40
        , IReadOnlyList<T>
#endif
    {
        /// <summary>
        /// An empty (initialized) instance of <see cref="Vector{T}"/>.
        /// </summary>
        public static readonly Vector<T> Empty = new Vector<T>(Array<T>.Empty);

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        internal T[] array;

        /// <summary>
        /// Gets the string to display in the debugger watches window for this instance.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                Vector<T> self = this;

                return self.IsDefault ? "Uninitialized" : $"{{{typeof(T).FullName}[{self.Length}]}}";
            }
        }

        /// <summary>
        /// Gets the number of elements in this collection.
        /// </summary>
        /// <remarks>
        /// "Length" chosen over "Count" so as to be able to reimplement LINQ's Count method.
        /// </remarks>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Length
        {
            get
            {
                // PERF: Not checking if initialised.
                return array.Length;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this struct was initialized without an actual array instance.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsDefault
        {
            get
            {
                return array == null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this struct is empty or uninitialized.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsDefaultOrEmpty
        {
            get
            {
                Vector<T> self = this; // Thread safety: prevent torn reads.

                return self.array == null || self.array.Length == 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this collection is empty.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsEmpty
        {
            get { return array.Length == 0; }
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                // PERF: Not checking if initialised.
                return array[index];
            }
        }

        /// <summary>
        /// Creates a new <see cref="Vector{T}"/> instance using the given array as backing store.
        /// </summary>
        /// <remarks>
        /// Keep internal to guarantee immutability.
        /// Do not abuse this constructor internally to wrap mutable
        /// arrays unless you are positive the array will never change.
        /// </remarks>
        internal Vector(T[] items)
        {
            array = items;
        }

        #region Enumerators

        /// <summary>
        /// Returns a struct enumerator that iterates through this collection.
        /// </summary>
        public ArrayEnumerator<T> GetEnumerator()
        {
            Vector<T> self = this; // Thread safety: prevent torn reads.

            self.ThrowNullRefIfNotInitialized();

            return new ArrayEnumerator<T>(self.array);
        }

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            Vector<T> self = this; // Thread safety: prevent torn reads.

            self.ThrowInvalidOperationIfNotInitialized();

            return EnumeratorObject.Create(self.array);
        }

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            Vector<T> self = this; // Thread safety: prevent torn reads.

            self.ThrowInvalidOperationIfNotInitialized();

            return EnumeratorObject.Create(self.array);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines whether the specified item exists in the vector.
        /// </summary>
        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        /// <summary>
        /// Copies the contents of this array to the specified array.
        /// </summary>
        public void CopyTo(T[] destination)
        {
            CopyTo(destination, 0);
        }

        /// <summary>
        /// Copies the contents of this array to the specified array.
        /// </summary>
        public void CopyTo(T[] destination, int destinationIndex)
        {
            Vector<T> self = this; // Thread safety: prevent torn reads.

            self.ThrowNullRefIfNotInitialized();

            Array.Copy(self.array, 0, destination, destinationIndex, self.array.Length);
        }

        /// <summary>
        /// Searches the vector for the specified item.
        /// </summary>
        public int IndexOf(T item)
        {
            Vector<T> self = this; // Thread safety: prevent torn reads.

            self.ThrowNullRefIfNotInitialized();

            return Array.IndexOf(self.array, item);
        }

        /// <summary>
        /// Copies the elements of this <see cref="Vector{T}"/> to a new array.
        /// </summary>
        public T[] ToArray()
        {
            Vector<T> self = this; // Thread safety: prevent torn reads.

            self.ThrowNullRefIfNotInitialized();

            if (self.IsEmpty) {
                return Array<T>.Empty;
            }

            T[] copy = new T[self.array.Length];

            Array.Copy(self.array, 0, copy, 0, self.array.Length);

            return copy;
        }

        #endregion

        #region Initialised checks

        /// <summary>
        /// Throws a null reference exception if the array field is null.
        /// </summary>
        internal void ThrowNullRefIfNotInitialized()
        {
            // Force NullReferenceException if array is null by touching its Length.
            // This way of checking has a nice property of requiring very little code
            // and not having any conditions/branches.
            // In a faulting scenario we are relying on hardware to generate the fault.
            // And in the non-faulting scenario (most common) the check is virtually free since
            // if we are going to do anything with the array, we will need Length anyways
            // so touching it, and potentially causing a cache miss, is not going to be an
            // extra expense.
            int unused = array.Length;
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the <see cref="array"/> field is null,
        /// i.e. the <see cref="IsDefault"/> property returns true.
        /// This is intended for explicitly implemented interface method and property implementations.
        /// </summary>
        private void ThrowInvalidOperationIfNotInitialized()
        {
            if (IsDefault) {
                throw new InvalidOperationException($"{nameof(Vector)} uninitialised.");
            }
        }

        #endregion

        #region IList<T> leftovers

        /// <summary>
        /// Gets the number of elements in this vector.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int ICollection<T>.Count
        {
            get
            {
                Vector<T> self = this; // Thread safety: prevent torn reads.

                self.ThrowInvalidOperationIfNotInitialized();

                return self.Length;
            }
        }

#if !NET_40
        /// <summary>
        /// Gets the number of elements in this vector.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        int IReadOnlyCollection<T>.Count
        {
            get
            {
                Vector<T> self = this; // Thread safety: prevent torn reads.

                self.ThrowInvalidOperationIfNotInitialized();

                return self.Length;
            }
        }
#endif
        /// <summary>
        /// Always returns true.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                ThrowInvalidOperationIfNotInitialized();

                return true;
            }
        }

        /// <summary>
        /// Gets the item at the specified index.
        /// </summary>
        T IList<T>.this[int index]
        {
            get
            {
                Vector<T> self = this;

                self.ThrowInvalidOperationIfNotInitialized();

                return self.array[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Suppressed.
        /// </summary>
        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Suppressed.
        /// </summary>
        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Suppressed.
        /// </summary>
        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Suppressed.
        /// </summary>
        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Suppressed.
        /// </summary>
        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Equality

        /// <summary>
        /// Returns true if the given instance is identical to this instance.
        /// </summary>
        public bool Equals(Vector<T> other)
        {
            return array == other.array;
        }

        /// <summary>
        /// Returns true if the given instance is identical to this instance.
        /// </summary>
        public override bool Equals(object obj)
        {
            // PERF: single field access (inside Equals), so no torn read check.
            return obj is Vector<T> && Equals((Vector<T>)obj);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        public override int GetHashCode()
        {
            return array.GetHashCode();
        }

        #endregion

        #region Operators

        /// <summary>
        /// Checks equality between two instances.
        /// </summary>
        public static bool operator ==(Vector<T> left, Vector<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks inequality between two instances.
        /// </summary>
        public static bool operator !=(Vector<T> left, Vector<T> right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Checks equality between two instances.
        /// </summary>
        public static bool operator ==(Vector<T>? left, Vector<T>? right)
        {
            return left.GetValueOrDefault().Equals(right.GetValueOrDefault());
        }

        /// <summary>
        /// Checks inequality between two instances.
        /// </summary>
        public static bool operator !=(Vector<T>? left, Vector<T>? right)
        {
            return !left.GetValueOrDefault().Equals(right.GetValueOrDefault());
        }

        #endregion

        /// <summary>
        /// An array enumerator that implements <see cref="IEnumerator{T}"/> pattern (including <see cref="IDisposable"/>).
        /// </summary>
        private sealed class EnumeratorObject : IEnumerator<T>
        {
            /// <summary>
            /// A shareable singleton for enumerating empty arrays.
            /// </summary>
            private static readonly IEnumerator<T> s_EmptyEnumerator =
                new EnumeratorObject(Array<T>.Empty);

            /// <summary>
            /// The array being enumerated.
            /// </summary>
            private readonly T[] _array;

            /// <summary>
            /// The currently enumerated position.
            /// </summary>
            /// <value>
            /// -1 before the first call to <see cref="MoveNext"/>.
            /// this.array.Length - 1 after MoveNext returns false.
            /// </value>
            private int _index;

            /// <summary>
            /// Initializes a new instance of the <see cref="EnumeratorObject"/> class.
            /// </summary>
            private EnumeratorObject(T[] array)
            {
                _index = -1;
                _array = array;
            }

            /// <summary>
            /// Gets the currently enumerated value.
            /// </summary>
            public T Current
            {
                get
                {
                    // this.index >= 0 && this.index < this.array.Length
                    // unsigned compare performs the range check above in one compare
                    if ((uint)_index < (uint)_array.Length)
                    {
                        return _array[_index];
                    }

                    // Before first or after last MoveNext.
                    throw new InvalidOperationException();
                }
            }

            /// <summary>
            /// Gets the currently enumerated value.
            /// </summary>
            object IEnumerator.Current
            {
                get { return Current; }
            }

            /// <summary>
            /// If another item exists in the array, advances to the next value to be enumerated.
            /// </summary>
            /// <returns><c>true</c> if another item exists in the array; <c>false</c> otherwise.</returns>
            public bool MoveNext()
            {
                int newIndex = _index + 1;
                int length = _array.Length;

                // unsigned math is used to prevent false positive if index + 1 overflows.
                if ((uint)newIndex <= (uint)length)
                {
                    _index = newIndex;
                    return (uint)newIndex < (uint)length;
                }

                return false;
            }

            /// <summary>
            /// Resets enumeration to the start of the array.
            /// </summary>
            void IEnumerator.Reset()
            {
                _index = -1;
            }

            /// <summary>
            /// Disposes this enumerator.
            /// </summary>
            /// <remarks>
            /// Currently has no action.
            /// </remarks>
            public void Dispose()
            {
                // we do not have any native or disposable resources.
                // nothing to do here.
            }

            /// <summary>
            /// Creates an enumerator for the specified array.
            /// </summary>
            internal static IEnumerator<T> Create(T[] array)
            {
                if (array.Length != 0)
                {
                    return new EnumeratorObject(array);
                }
                else
                {
                    return s_EmptyEnumerator;
                }
            }
        }
    }
}

namespace System.Linq
{
    /// <summary>
    /// LINQ extension method overrides that offer greater efficiency for <see cref="Vector{T}"/> than the standard LINQ methods
    /// </summary>
    public static class VectorExtensions
    {
        private static readonly byte[] TwoElementArray = new byte[2];

        #region Vector<T> extensions

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        public static IEnumerable<TResult> Select<T, TResult>(this Vector<T> vector, Func<T, TResult> selector)
        {
            vector.ThrowNullRefIfNotInitialized();

            // LINQ Select/Where have optimized treatment for arrays.
            // They also do not modify the source arrays or expose them to modifications.
            // Therefore we will just apply Select/Where to the underlying this.array array.
            return vector.array.Select(selector);
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{T}"/>,
        /// flattens the resulting sequences into one sequence, and invokes a result
        /// selector function on each element therein.
        /// </summary>
        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
            this Vector<TSource> vector,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            vector.ThrowNullRefIfNotInitialized();
            if (collectionSelector == null || resultSelector == null)
            {
                // throw the same exception as would LINQ
                return Enumerable.SelectMany(vector.array, collectionSelector, resultSelector);
            }

            // This SelectMany overload is used by the C# compiler for a query of the form:
            //     from i in vector
            //     from j in anotherCollection
            //     select Something(i, j);
            // SelectMany accepts an IEnumerable<TSource>, and Vector<TSource> is a struct.
            // By having a special implementation of SelectMany that operates on the Vector's
            // underlying array, we can avoid a few allocations, in particular for the boxed
            // immutable array object that would be allocated when it's passed as an IEnumerable<T>, 
            // and for the EnumeratorObject that would be allocated when enumerating the boxed array.

            return vector.Length == 0 ?
                Enumerable.Empty<TResult>() :
                SelectManyIterator(vector, collectionSelector, resultSelector);
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        public static IEnumerable<T> Where<T>(this Vector<T> vector, Func<T, bool> predicate)
        {
            vector.ThrowNullRefIfNotInitialized();

            // LINQ Select/Where have optimized treatment for arrays.
            // They also do not modify the source arrays or expose them to modifications.
            // Therefore we will just apply Select/Where to the underlying this.array array.
            return vector.array.Where(predicate);
        }

        /// <summary>
        /// Gets a value indicating whether any elements are in this collection.
        /// </summary>
        public static bool Any<T>(this Vector<T> vector)
        {
            return vector.Length > 0;
        }

        /// <summary>
        /// Gets a value indicating whether any elements are in this collection
        /// that match a given condition.
        /// </summary>
        public static bool Any<T>(this Vector<T> vector, Func<T, bool> predicate)
        {
            Requires.NotNull(predicate, "predicate");

            foreach (var v in vector.array)
            {
                if (predicate(v))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether all elements in this collection
        /// match a given condition.
        /// </summary>
        public static bool All<T>(this Vector<T> vector, Func<T, bool> predicate)
        {
            Requires.NotNull(predicate, "predicate");

            foreach (var v in vector.array)
            {
                if (!predicate(v))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether two sequences are equal according to an equality comparer.
        /// </summary>
        public static bool SequenceEqual<TDerived, TBase>(this Vector<TBase> vector, Vector<TDerived> items, IEqualityComparer<TBase> comparer = null) where TDerived : TBase
        {
            vector.ThrowNullRefIfNotInitialized();
            items.ThrowNullRefIfNotInitialized();
            if (object.ReferenceEquals(vector.array, items.array))
            {
                return true;
            }

            if (vector.Length != items.Length)
            {
                return false;
            }

            if (comparer == null)
            {
                comparer = EqualityComparer<TBase>.Default;
            }

            for (int i = 0; i < vector.Length; i++)
            {
                if (!comparer.Equals(vector.array[i], items.array[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether two sequences are equal according to an equality comparer.
        /// </summary>
        public static bool SequenceEqual<TDerived, TBase>(this Vector<TBase> vector, IEnumerable<TDerived> items, IEqualityComparer<TBase> comparer = null) where TDerived : TBase
        {
            Requires.NotNull(items, "items");

            if (comparer == null)
            {
                comparer = EqualityComparer<TBase>.Default;
            }

            int i = 0;
            int n = vector.Length;
            foreach (var item in items)
            {
                if (i == n)
                {
                    return false;
                }

                if (!comparer.Equals(vector[i], item))
                {
                    return false;
                }

                i++;
            }

            return i == n;
        }

        /// <summary>
        /// Determines whether two sequences are equal according to an equality comparer.
        /// </summary>
        public static bool SequenceEqual<TDerived, TBase>(this Vector<TBase> vector, Vector<TDerived> items, Func<TBase, TBase, bool> predicate) where TDerived : TBase
        {
            Requires.NotNull(predicate, "predicate");
            vector.ThrowNullRefIfNotInitialized();
            items.ThrowNullRefIfNotInitialized();

            if (object.ReferenceEquals(vector.array, items.array))
            {
                return true;
            }

            if (vector.Length != items.Length)
            {
                return false;
            }

            for (int i = 0, n = vector.Length; i < n; i++)
            {
                if (!predicate(vector[i], items[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Applies an accumulator function over a sequence.
        /// </summary>
        public static T Aggregate<T>(this Vector<T> vector, Func<T, T, T> func)
        {
            Requires.NotNull(func, "func");

            if (vector.Length == 0)
            {
                return default(T);
            }

            var result = vector[0];
            for (int i = 1, n = vector.Length; i < n; i++)
            {
                result = func(result, vector[i]);
            }

            return result;
        }

        /// <summary>
        /// Applies an accumulator function over a sequence.
        /// </summary>
        public static TAccumulate Aggregate<TAccumulate, T>(this Vector<T> vector, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
        {
            Requires.NotNull(func, "func");

            var result = seed;
            foreach (var v in vector.array)
            {
                result = func(result, v);
            }

            return result;
        }

        /// <summary>
        /// Applies an accumulator function over a sequence.
        /// </summary>
        public static TResult Aggregate<TAccumulate, TResult, T>(this Vector<T> vector, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            Requires.NotNull(resultSelector, "resultSelector");

            return resultSelector(Aggregate(vector, seed, func));
        }

        /// <summary>
        /// Returns the element at a specified index in a sequence.
        /// </summary>
        public static T ElementAt<T>(this Vector<T> vector, int index)
        {
            return vector[index];
        }

        /// <summary>
        /// Returns the element at a specified index in a sequence or a default value if the index is out of range.
        /// </summary>
        public static T ElementAtOrDefault<T>(this Vector<T> vector, int index)
        {
            if (index < 0 || index >= vector.Length)
            {
                return default(T);
            }

            return vector[index];
        }

        /// <summary>
        /// Returns the first element in a sequence that satisfies a specified condition.
        /// </summary>
        public static T First<T>(this Vector<T> vector, Func<T, bool> predicate)
        {
            Requires.NotNull(predicate, "predicate");

            foreach (var v in vector.array)
            {
                if (predicate(v))
                {
                    return v;
                }
            }

            // Throw the same exception that LINQ would.
            return Enumerable.Empty<T>().First();
        }

        /// <summary>
        /// Returns the first element in a sequence that satisfies a specified condition.
        /// </summary>
        public static T First<T>(this Vector<T> vector)
        {
            // In the event of an empty array, generate the same exception 
            // that the linq extension method would.
            return vector.Length > 0
                ? vector[0]
                : Enumerable.First(vector.array);
        }

        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        public static T FirstOrDefault<T>(this Vector<T> vector)
        {
            return vector.array.Length > 0 ? vector.array[0] : default(T);
        }

        /// <summary>
        /// Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.
        /// </summary>
        public static T FirstOrDefault<T>(this Vector<T> vector, Func<T, bool> predicate)
        {
            Requires.NotNull(predicate, "predicate");

            foreach (var v in vector.array)
            {
                if (predicate(v))
                {
                    return v;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Returns the last element of a sequence.
        /// </summary>
        public static T Last<T>(this Vector<T> vector)
        {
            // In the event of an empty array, generate the same exception 
            // that the linq extension method would.
            return vector.Length > 0
                ? vector[vector.Length - 1]
                : Enumerable.Last(vector.array);
        }

        /// <summary>
        /// Returns the last element of a sequence that satisfies a specified condition.
        /// </summary>
        public static T Last<T>(this Vector<T> vector, Func<T, bool> predicate)
        {
            Requires.NotNull(predicate, "predicate");

            for (int i = vector.Length - 1; i >= 0; i--)
            {
                if (predicate(vector[i]))
                {
                    return vector[i];
                }
            }

            // Throw the same exception that LINQ would.
            return Enumerable.Empty<T>().Last();
        }

        /// <summary>
        /// Returns the last element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        public static T LastOrDefault<T>(this Vector<T> vector)
        {
            vector.ThrowNullRefIfNotInitialized();

            return vector.array.LastOrDefault();
        }

        /// <summary>
        /// Returns the last element of a sequence that satisfies a condition or a default value if no such element is found.
        /// </summary>
        public static T LastOrDefault<T>(this Vector<T> vector, Func<T, bool> predicate)
        {
            Requires.NotNull(predicate, "predicate");

            for (int i = vector.Length - 1; i >= 0; i--)
            {
                if (predicate(vector[i]))
                {
                    return vector[i];
                }
            }

            return default(T);
        }

        /// <summary>
        /// Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>
        public static T Single<T>(this Vector<T> vector)
        {
            vector.ThrowNullRefIfNotInitialized();

            return vector.array.Single();
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
        /// </summary>
        public static T Single<T>(this Vector<T> vector, Func<T, bool> predicate)
        {
            Requires.NotNull(predicate, "predicate");

            var first = true;
            var result = default(T);
            foreach (var v in vector.array)
            {
                if (predicate(v))
                {
                    if (!first)
                    {
                        TwoElementArray.Single(); // throw the same exception as LINQ would
                    }

                    first = false;
                    result = v;
                }
            }

            if (first)
            {
                Enumerable.Empty<T>().Single(); // throw the same exception as LINQ would
            }

            return result;
        }

        /// <summary>
        /// Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        public static T SingleOrDefault<T>(this Vector<T> vector)
        {
            vector.ThrowNullRefIfNotInitialized();

            return vector.array.SingleOrDefault();
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.
        /// </summary>
        public static T SingleOrDefault<T>(this Vector<T> vector, Func<T, bool> predicate)
        {
            Requires.NotNull(predicate, "predicate");

            var first = true;
            var result = default(T);
            foreach (var v in vector.array)
            {
                if (predicate(v))
                {
                    if (!first)
                    {
                        TwoElementArray.Single(); // throw the same exception as LINQ would
                    }

                    first = false;
                    result = v;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a dictionary based on the contents of this array.
        /// </summary>
        public static Dictionary<TKey, T> ToDictionary<TKey, T>(this Vector<T> vector, Func<T, TKey> keySelector)
        {
            return ToDictionary(vector, keySelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Creates a dictionary based on the contents of this array.
        /// </summary>
        public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement, T>(this Vector<T> vector, Func<T, TKey> keySelector, Func<T, TElement> elementSelector)
        {
            return ToDictionary(vector, keySelector, elementSelector, EqualityComparer<TKey>.Default);
        }

        /// <summary>
        /// Creates a dictionary based on the contents of this array.
        /// </summary>
        public static Dictionary<TKey, T> ToDictionary<TKey, T>(this Vector<T> vector, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Requires.NotNull(keySelector, "keySelector");

            var result = new Dictionary<TKey, T>(comparer);
            foreach (var v in vector)
            {
                result.Add(keySelector(v), v);
            }

            return result;
        }

        /// <summary>
        /// Creates a dictionary based on the contents of this array.
        /// </summary>
        public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement, T>(this Vector<T> vector, Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            Requires.NotNull(keySelector, "keySelector");
            Requires.NotNull(elementSelector, "elementSelector");

            var result = new Dictionary<TKey, TElement>(vector.Length, comparer);
            foreach (var v in vector.array)
            {
                result.Add(keySelector(v), elementSelector(v));
            }

            return result;
        }

        /// <summary>
        /// Copies the contents of this array to a mutable array.
        /// </summary>
        public static T[] ToArray<T>(this Vector<T> vector)
        {
            vector.ThrowNullRefIfNotInitialized();

            return vector.array.ToArray();
        }

        #endregion

        #region Private Implementation Details
        /// <summary>Provides the core iterator implementation of <see cref="SelectMany"/>.</summary>
        private static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(
            this Vector<TSource> vector,
            Func<TSource, IEnumerable<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            foreach (TSource item in vector.array)
            {
                foreach (TCollection result in collectionSelector(item))
                {
                    yield return resultSelector(item, result);
                }
            }
        }
        #endregion

        static class Requires
        {
            public static void NotNull<T>(T value, string paramName) where T : class
            {
                if (value == null) throw new ArgumentNullException(paramName);
            }
        }
    }
}