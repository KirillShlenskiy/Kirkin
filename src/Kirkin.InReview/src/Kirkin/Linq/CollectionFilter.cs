using System;
using System.Collections.Generic;
using System.Linq;

namespace Kirkin.Linq
{
    /// <summary>
    /// Collection filter factory and extension methods.
    /// </summary>
    /// <remarks>
    /// These <see cref="ICollectionFilter{T}"/> implementations offer
    /// superior performance when composing multiple filters compared to chained
    /// System.Linq.Enumerable.Intersect/Union methods which cause extra allocations
    /// and intermediate collection iterations along the way (also losing sort order).
    /// </remarks>
    public static class CollectionFilter
    {
        #region Public factory methods

        /// <summary>
        /// Creates a filter optimised for processing large collections of objects.
        /// Use for filters which are expensive to initialise (i.e. have joins).
        /// </summary>
        public static ICollectionFilter<T> Create<T>(Func<IEnumerable<T>, IEnumerable<T>> filterFunc)
        {
            if (filterFunc == null) throw new ArgumentNullException("filterFunc");

            return new DelegateFilter<T>(filterFunc);
        }

        /// <summary>
        /// Creates a filter optimised for processing large collections of objects.
        /// Use for filters with minimal pre-processing required.
        /// </summary>
        public static ICollectionFilter<T> FromPredicate<T>(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException("predicate");

            return new PredicateFilter<T>(predicate);
        }

        /// <summary>
        /// Creates a multi-stage collection filter which combines
        /// multiple filters ultimately producing a subset
        /// of items which satisfy all filter conditions.
        /// </summary>
        public static ICollectionFilter<T> MustSatisfyAll<T>(params ICollectionFilter<T>[] filters)
        {
            if (filters == null) throw new ArgumentNullException(nameof(filters));
            if (filters.Length == 0) throw new ArgumentException("Combined filter collection cannot be empty.");

            return new MustSatisfyAllFilter<T>(filters);
        }

        /// <summary>
        /// Creates a multi-stage collection filter which combines
        /// multiple filters ultimately producing a superset
        /// of items which satisfy any of the filter conditions.
        /// </summary>
        public static ICollectionFilter<T> MustSatisfyAny<T>(params ICollectionFilter<T>[] filters)
        {
            if (filters == null) throw new ArgumentNullException(nameof(filters));
            if (filters.Length == 0) throw new ArgumentException("Combined filter collection cannot be empty.");

            return new MustSatisfyAnyFilter<T>(filters);
        }

        /// <summary>
        /// Creates a pass-through filter which does not perform any filtering on the collection.
        /// </summary>
        public static ICollectionFilter<T> Unfiltered<T>()
        {
            return new PassThroughFilter<T>();
        }

        #endregion

        #region Public extension methods

        /// <summary>
        /// Filters the given collection based on its projection.
        /// </summary>
        public static IEnumerable<TElement> FilterByProjection<TElement, TValue>(this ICollectionFilter<TValue> filter, IEnumerable<TElement> collection, Func<TElement, TValue> selector)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            List<KeyValuePair<TElement, TValue>> elementsAndValues = new List<KeyValuePair<TElement, TValue>>();

            foreach (TElement element in collection) {
                elementsAndValues.Add(new KeyValuePair<TElement, TValue>(element, selector(element)));
            }

            HashSet<TValue> matches = new HashSet<TValue>(filter.Filter(elementsAndValues.Select(ev => ev.Value)));

            foreach (KeyValuePair<TElement, TValue> elementAndValue in elementsAndValues)
            {
                if (matches.Contains(elementAndValue.Value)) {
                    yield return elementAndValue.Key;
                }
            }
        }

        /// <summary>
        /// Creates a multi-stage collection filter which combines
        /// multiple filters ultimately producing a subset
        /// of items which satisfy all filter conditions.
        /// </summary>
        public static ICollectionFilter<T> And<T>(this ICollectionFilter<T> filter, Func<T, bool> predicate)
        {
            return filter.And(FromPredicate(predicate));
        }

        /// <summary>
        /// Creates a multi-stage collection filter which combines
        /// multiple filters ultimately producing a subset
        /// of items which satisfy all filter conditions.
        /// </summary>
        public static ICollectionFilter<T> And<T>(this ICollectionFilter<T> filter, ICollectionFilter<T> other)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (other == null) throw new ArgumentNullException(nameof(other));

            ICollectionFilter<T>[] combinedFilters;
            MustSatisfyAllFilter<T> mustSatisfyAll = filter as MustSatisfyAllFilter<T>;

            if (mustSatisfyAll != null)
            {
                combinedFilters = new ICollectionFilter<T>[mustSatisfyAll.Filters.Length + 1];
                Array.Copy(mustSatisfyAll.Filters, 0, combinedFilters, 0, mustSatisfyAll.Filters.Length);
                combinedFilters[combinedFilters.Length - 1] = other;
            }
            else
            {
                combinedFilters = new[] { filter, other };
            }

            return new MustSatisfyAllFilter<T>(combinedFilters);
        }

        /// <summary>
        /// Creates a multi-stage collection filter which combines
        /// multiple filters ultimately producing a superset
        /// of items which satisfy any of the filter conditions.
        /// </summary>
        public static ICollectionFilter<T> Or<T>(this ICollectionFilter<T> filter, Func<T, bool> predicate)
        {
            return filter.Or(FromPredicate(predicate));
        }

        /// <summary>
        /// Creates a multi-stage collection filter which combines
        /// multiple filters ultimately producing a superset
        /// of items which satisfy any of the filter conditions.
        /// </summary>
        public static ICollectionFilter<T> Or<T>(this ICollectionFilter<T> filter, ICollectionFilter<T> other)
        {
            ICollectionFilter<T>[] combinedFilters;
            MustSatisfyAnyFilter<T> mustSatisfyAny = filter as MustSatisfyAnyFilter<T>;

            if (mustSatisfyAny != null)
            {
                combinedFilters = new ICollectionFilter<T>[mustSatisfyAny.Filters.Length + 1];
                Array.Copy(mustSatisfyAny.Filters, 0, combinedFilters, 0, mustSatisfyAny.Filters.Length);
                combinedFilters[combinedFilters.Length - 1] = other;
            }
            else
            {
                combinedFilters = new[] { filter, other };
            }

            return new MustSatisfyAnyFilter<T>(combinedFilters);
        }

        #endregion

        #region Implementation

        sealed class DelegateFilter<T> : ICollectionFilter<T>
        {
            private readonly Func<IEnumerable<T>, IEnumerable<T>> FilterFunc;

            public DelegateFilter(Func<IEnumerable<T>, IEnumerable<T>> filterFunc)
            {
                FilterFunc = filterFunc;
            }

            public IEnumerable<T> Filter(IEnumerable<T> collection)
            {
                return FilterFunc(collection);
            }
        }

        sealed class MustSatisfyAllFilter<T> : ICollectionFilter<T>
        {
            internal readonly ICollectionFilter<T>[] Filters;

            internal MustSatisfyAllFilter(ICollectionFilter<T>[] filters)
            {
                Filters = filters;
            }

            public IEnumerable<T> Filter(IEnumerable<T> collection)
            {
                List<T> list = new List<T>(collection);
                HashSet<T> result = new HashSet<T>(list);

                foreach (ICollectionFilter<T> stage in Filters) {
                    result.IntersectWith(stage.Filter(result));
                }

                foreach (T item in list)
                {
                    if (result.Contains(item)) {
                        yield return item;
                    }
                }
            }
        }

        sealed class MustSatisfyAnyFilter<T> : ICollectionFilter<T>
        {
            internal readonly ICollectionFilter<T>[] Filters;

            internal MustSatisfyAnyFilter(ICollectionFilter<T>[] filters)
            {
                Filters = filters;
            }

            public IEnumerable<T> Filter(IEnumerable<T> collection)
            {
                List<T> list = new List<T>(collection);
                HashSet<T> result = new HashSet<T>(list);

                foreach (ICollectionFilter<T> stage in Filters) {
                    result.UnionWith(stage.Filter(result));
                }

                foreach (T item in list)
                {
                    if (result.Contains(item)) {
                        yield return item;
                    }
                }
            }
        }

        sealed class PassThroughFilter<T> : ICollectionFilter<T>
        {
            public IEnumerable<T> Filter(IEnumerable<T> collection)
            {
                return collection;
            }
        }

        sealed class PredicateFilter<T> : ICollectionFilter<T>
        {
            private readonly Func<T, bool> Predicate;

            public PredicateFilter(Func<T, bool> predicate)
            {
                Predicate = predicate;
            }

            public IEnumerable<T> Filter(IEnumerable<T> collection)
            {
                return collection.Where(Predicate);
            }
        }

        #endregion
    }
}