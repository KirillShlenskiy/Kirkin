using System;
using System.Collections.Generic;

namespace Kirkin.Functional
{
    /// <summary>
    /// Simple optional value holder.
    /// </summary>
    public struct Option<T>
        : IEquatable<T>
        , IEquatable<Option<T>>
    {
        #region Static members

        /// <summary>
        /// Returns an empty Option{T} value (with HasValue = false).
        /// </summary>
        public static Option<T> None
        {
            get { return default(Option<T>); }
        }

        #endregion

        #region Fields, properties, constructor and basic plumbing

        private readonly bool __hasValue;
        private readonly T __value;

        /// <summary>
        /// True if the value was specified. Otherwise false.
        /// </summary>
        public bool HasValue
        {
            get { return __hasValue; }
        }

        /// <summary>
        /// Value which was specified when this object was created
        /// (or default of type T if no value was specified).
        /// </summary>
        public T Value
        {
            get
            {
                if (!HasValue) {
                    throw new InvalidOperationException("Value is undefined when HasValue is false.");
                }

                return __value;
            }
        }

        /// <summary>
        /// Creates a new instance of Option{T} with the given value.
        /// Regardless of what the value is, HasValue will be true for this instance.
        /// </summary>
        public Option(T value)
        {
            __value = value;
            __hasValue = !object.ReferenceEquals(value, null);
        }

        /// <summary>
        /// Returns the text representation of the value of the current Option{T} object.
        /// </summary>
        public override string ToString()
        {
            return HasValue ? __value.ToString() : "";
        }

        #endregion

        #region Safe value retrieval

        /// <summary>
        /// Retrieves the value of the current Option{T} object, or the object's default value.
        /// </summary>
        public T GetValueOrDefault()
        {
            return __value;
        }

        /// <summary>
        /// Retrieves the value of the current Option{T} object, or the specified default value.
        /// </summary>
        public T GetValueOrDefault(T defaultValue)
        {
            return HasValue ? __value : defaultValue;
        }

        /// <summary>
        /// Attempts to retrieve the underlying object. The return
        /// value indicates whether the operation was successful.
        /// </summary>
        public bool TryGetValue(out T value)
        {
            if (HasValue)
            {
                value = __value;
                return true;
            }

            value = default(T);
            return false;
        }

        #endregion

        #region Equality and GetHashCode

        // Below methods are largely copied from Sasa.Option.

        /// <summary>
        /// Indicates whether the current Option{T} object is equal to a specified object.
        /// </summary>
        public bool Equals(T other)
        {
            return
                other == null && !HasValue ||
                HasValue && EqualityComparer<T>.Default.Equals(__value, other);
        }

        /// <summary>
        /// Indicates whether the current Option{T} object is equal to a specified Option{T}.
        /// </summary>
        public bool Equals(Option<T> other)
        {
            return
                HasValue && other.HasValue && EqualityComparer<T>.Default.Equals(__value, other.__value) ||
                !HasValue && !other.HasValue;
        }

        /// <summary>
        /// Indicates whether the current Option{T} object is equal to a specified object.
        /// </summary>
        public override bool Equals(object obj)
        {
            return
                obj == null && !HasValue ||
                obj is T && Equals((T)obj) ||
                obj is Option<T> && Equals((Option<T>)obj);
        }

        /// <summary>
        /// Returns the hash value of the underlying object.
        /// </summary>
        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(__value);
        }

        #endregion

        #region Projections and side-effects

        /// <summary>
        /// Performs the given action on the underlying
        /// object if this Option{t} has a value.
        /// </summary>
        public void Do(Action<T> action)
        {
            if (HasValue) {
                action(__value);
            }
        }

        /// <summary>
        /// Creates a projection to the given type if this Option{T} has a value.
        /// </summary>
        public Option<R> Select<R>(Func<T, R> projection)
        {
            if (!HasValue) {
                return Option<R>.None;
            }

            return projection(__value);
        }

        #endregion

        #region Conversion operators

        /// <summary>
        /// Creates a new Option{T} object with the given value.
        /// </summary>
        public static implicit operator Option<T>(T value)
        {
            return new Option<T>(value);
        }

        /// <summary>
        /// Returns the value of a specified Option{T}.
        /// </summary>
        public static explicit operator T(Option<T> option)
        {
            return option.Value;
        }

        #endregion

        #region Convenience operators

        /// <summary>
        /// Checks the given Option{T} instances for equality.
        /// </summary>
        public static bool operator ==(Option<T> left, Option<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Checks the given Option{T} instances for inequality.
        /// </summary>
        public static bool operator !=(Option<T> left, Option<T> right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Null coalescing operator equivalent where
        /// the right hand side is another Option{T}.
        /// </summary>
        public static Option<T> operator |(Option<T> x, Option<T> y)
        {
            return x.HasValue ? x : y;
        }

        /// <summary>
        /// Null coalescing operator equivalent where
        /// the right hand side is a concrete value.
        /// </summary>
        public static T operator %(Option<T> option, T value)
        {
            return option.GetValueOrDefault(value);
        }

        /// <summary>
        /// Proxies the given Option{T}'s HasValue property
        /// so that it can participate in boolean comparisons.
        /// </summary>
        public static bool operator true(Option<T> option)
        {
            return option.HasValue;
        }

        /// <summary>
        /// Proxies the given Option{T}'s HasValue property
        /// so that it can participate in boolean comparisons.
        /// </summary>
        public static bool operator false(Option<T> option)
        {
            return !option.HasValue;
        }

        #endregion
    }

    /// <summary>
    /// Static and extension methods for Option{T}.
    /// </summary>
    public static class Option
    {
        /// <summary>
        /// Converts Option{T} to an equivalent Nullable{T} value.
        /// </summary>
        public static T? ToNullable<T>(this Option<T> option)
            where T : struct
        {
            T value;
            return option.TryGetValue(out value) ? value : new T?();
        }

        /// <summary>
        /// Wraps the object in an Option{T}.
        /// </summary>
        public static Option<T> ToOption<T>(this T value)
        {
            return new Option<T>(value);
        }
    }
}