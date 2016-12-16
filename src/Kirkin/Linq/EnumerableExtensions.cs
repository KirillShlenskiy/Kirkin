using System;
using System.Collections.Generic;

#if !NET_40
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Kirkin.Linq
{
    /// <summary>
    /// Extension methods for types which implement the IEnumerable{T} interface.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Produces a new sequence by iterating through all items in the
        /// current collection and appending the given item at the end.
        /// </summary>
        /// <remarks>
        /// Initially called "Add" but renamed to avoid confusion with
        /// the mutable equivalent defined on many collection types.
        /// </remarks>
        internal static IEnumerable<T> Append<T>(this IEnumerable<T> collection, T item)
        {
            foreach (T element in collection) {
                yield return element;
            }

            yield return item;
        }

        /// <summary>
        /// Applies the given action to each element in the collection.
        /// </summary>
        public static IEnumerable<T> Do<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T item in collection)
            {
                action(item);

                yield return item;
            }
        }

        /// <summary>
        /// Returns the index of the given element inside
        /// the collection or -1 if it cannot be found.
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> collection, T itemToSeek)
        {
            // Optimisation.
            IList<T> list = collection as IList<T>;

            if (list != null) {
                return list.IndexOf(itemToSeek);
            }

            // Seek.
            int index = 0;

            foreach (T item in collection)
            {
                if (Equals(item, itemToSeek)) {
                    return index;
                }

                index++;
            }

            return -1;
        }

        /// <summary>
        /// Breaks up the given sequence into smaller
        /// materialized sequences of the given size.
        /// </summary>
        public static IEnumerable<T[]> Chunkify<T>(this IEnumerable<T> collection, int chunkSize)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (chunkSize < 1) throw new ArgumentException("chunkSize");

            T[] chunk = null;
            int count = 0;

            foreach (T obj in collection)
            {
                if (count == 0) {
                    chunk = new T[chunkSize];
                }

                chunk[count++] = obj;

                if (count == chunkSize)
                {
                    yield return chunk;

                    count = 0;
                }
            }

            if (count != 0)
            {
                if (chunk.Length == count) {
                    yield return chunk;
                }

                // Resize required.
                T[] tmp = new T[count];

                Array.Copy(chunk, 0, tmp, 0, count);

                yield return tmp;
            }
        }

        /// <summary>
        /// Returns the first item whose key is the smallest according to the default comparer.
        /// Similar to collection.OrderBy(keySelector).FirstOrDefault() without the extra collection allocation and sorting,
        /// or collection.FirstOrDefault(i => keySelector(i) == collection.Min(keySelector)).
        /// </summary>
        public static TElement FirstOrDefaultWithMin<TElement, TKey>(this IEnumerable<TElement> collection, Func<TElement, TKey> keySelector)
        {
            return FirstOrDefaultWithMin(collection, keySelector, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Returns the first item whose key is the smallest according to the specified comparer.
        /// Similar to collection.OrderBy(keySelector, comparer).FirstOrDefault() without the extra collection allocation and sorting.
        /// </summary>
        public static TElement FirstOrDefaultWithMin<TElement, TKey>(this IEnumerable<TElement> collection, Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            Value<TElement, TKey> current = new Value<TElement, TKey>();

            foreach (TElement element in collection)
            {
                TKey key = keySelector(element);

                if (!current.IsSet || comparer.Compare(key, current.Key) < 0) {
                    current = new Value<TElement, TKey>(element, key);
                }
            }

            return current.Element;
        }

        /// <summary>
        /// Returns the last item whose key is the largest according to the default comparer.
        /// Similar to collection.OrderBy(keySelector).LastOrDefault() without the extra collection allocation and sorting,
        /// or collection.LastOrDefault(i => keySelector(i) == collection.Max(keySelector)).
        /// </summary>
        public static TElement LastOrDefaultWithMax<TElement, TKey>(this IEnumerable<TElement> collection, Func<TElement, TKey> keySelector)
        {
            return LastOrDefaultWithMax(collection, keySelector, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Returns the last item whose key is the largest according to the specified comparer.
        /// Similar to collection.OrderBy(keySelector, comparer).LastOrDefault() without the extra collection allocation and sorting.
        /// </summary>
        public static TElement LastOrDefaultWithMax<TElement, TKey>(this IEnumerable<TElement> collection, Func<TElement, TKey> keySelector, IComparer<TKey> comparer)
        {
            Value<TElement, TKey> current = new Value<TElement, TKey>();

            foreach (TElement element in collection)
            {
                TKey key = keySelector(element);

                if (!current.IsSet || comparer.Compare(key, current.Key) >= 0) {
                    current = new Value<TElement, TKey>(element, key);
                }
            }

            return current.Element;
        }

        private struct Value<TElement, TKey>
        {
            internal bool IsSet;
            internal TElement Element;
            internal TKey Key;

            internal Value(TElement element, TKey key)
            {
                IsSet = true;
                Element = element;
                Key = key;
            }
        }

#if !NET_40
        /// <summary>
        /// Returns an enumerable which eagerly iterates through the given collection on
        /// the thread pool and stores resulting elements in a buffer before yielding them.
        /// </summary>
        /// <remarks>
        /// As it stands, the buffering only starts at the first MoveNext call.
        /// Moving forward, this behaviour is subject to change.
        /// </remarks>
        public static IEnumerable<TElement> LookAhead<TElement>(this IEnumerable<TElement> collection, int bufferSize = 1)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (bufferSize < 1) throw new ArgumentOutOfRangeException("bufferSize", "Minimum allowed buffer size is 1.");

            using (BlockingCollection<TElement> buffer = new BlockingCollection<TElement>(bufferSize))
            using (SemaphoreSlim semaphore = new SemaphoreSlim(bufferSize, bufferSize))
            {
                // Greedy item grab.
                Task producer = Task.Run(async () =>
                {
                    try
                    {
                        foreach (TElement item in collection)
                        {
                            await semaphore.WaitAsync().ConfigureAwait(false);

                            lock (buffer)
                            {
                                // IsAddingCompleted check required to prevent Add throwing
                                // because the main thread has already called CompleteAdding.
                                if (buffer.IsAddingCompleted) return;

                                buffer.Add(item);
                            }
                        }
                    }
                    finally
                    {
                        // No lock required as all additions are finished by now.
                        // Multiple/concurrent calls to CompleteAdding are fine.
                        buffer.CompleteAdding();
                    }
                });

                try
                {
                    foreach (TElement item in buffer.GetConsumingEnumerable())
                    {
                        yield return item;

                        // Wake the producer. This will not be called if the consumer short-circuits.
                        // In that event Release will be called after CompleteAdding inside the finally block.
                        semaphore.Release();
                    }
                }
                finally
                {
                    // Must be in "finally" as otherwise it
                    // won't be called if we break out early.
                    lock (buffer) {
                        buffer.CompleteAdding();
                    }

                    if (semaphore.CurrentCount < bufferSize)
                    {
                        // Only required if we broke out early. Wakes the producer only for it
                        // to immediately hit the buffer.IsAddingCompleted check and break out.
                        semaphore.Release();
                    }

                    // Block until completion and observe exceptions.
                    producer.GetAwaiter().GetResult();
                }
            }
        }
#endif
        /// <summary>
        /// Produces a new sequence by first yielding the given item and
        /// then iterating through all items in the current collection.
        /// </summary>
        internal static IEnumerable<T> Prepend<T>(this IEnumerable<T> collection, T item)
        {
            yield return item;

            foreach (T element in collection) {
                yield return element;
            }
        }
    }
}