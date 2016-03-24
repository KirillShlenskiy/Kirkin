using System;

namespace Kirkin
{
    /// <summary>
    /// System.String wrapper struct whose Value is
    /// guaranteed to never be null. In the event
    /// that a null reference is supplied when this
    /// instance is created, it will be treated as
    /// an empty string. This type is implicitly
    /// convertible to and from System.String.
    /// </summary>
    internal struct NonNullableString
        : IEquatable<NonNullableString>
        , IEquatable<string>
        , IConvertible // SqlParameterCollection support.
    {
        /// <summary>
        /// Original string supplied when this instance was created.
        /// </summary>
        private readonly string OriginalValue;

        /// <summary>
        /// Returns the string equivalent of
        /// this instance. Can never be null.
        /// </summary>
        public string Value
        {
            get
            {
                // C# specs make no guarantee that the null-coalescing
                // operator copies the reference before checking it for
                // null, so we will do it ourselves to ensure that access to
                // Value is thread-safe in any and all .NET implementations.
                // This ensures that "this" is dereferenced only once and
                // prevents torn reads if the variable or field holding this
                // struct is reassigned during execution, causing the wrong
                // OriginalValue to be seen on second access, and ultimately
                // allowing null to be returned.
                var value = this.OriginalValue;

                return value ?? string.Empty;
            }
        }

        /// <summary>
        /// Creates a new value from the given string.
        /// </summary>
        public NonNullableString(string value)
        {
            this.OriginalValue = value;
        }

        /// <summary>
        /// Checks if this instance's value is
        /// equal to the other instance's value.
        /// </summary>
        public bool Equals(NonNullableString other)
        {
            return this.Value == other.Value;
        }

        /// <summary>
        /// Checks if this instance's value
        /// is equal to the given string.
        /// </summary>
        public bool Equals(string other)
        {
            return string.Equals(this.Value, other);
        }

        /// <summary>
        /// Checks if this instance's value
        /// is equal to the given object.
        /// </summary>
        public override bool Equals(object obj)
        {
            // Thread safety: prevent torn reads.
            var self = this;

            if (obj is NonNullableString) {
                return self.Equals((NonNullableString)obj);
            }

            return self.Equals(obj as string);
        }

        /// <summary>
        /// Gets the hash code of this instance's value.
        /// </summary>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        /// <summary>
        /// Returns the string equivalent of
        /// this instance. Can never be null.
        /// </summary>
        public override string ToString()
        {
            return this.Value;
        }

        #region System.String proxies

        //public bool Contains(string value)
        //{
        //    return this.Value.Contains(value);
        //}

        //public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        //{
        //    this.Value.CopyTo(sourceIndex, destination, destinationIndex, count);
        //}

        //public bool EndsWith(string value)
        //{
        //    return this.Value.EndsWith(value);
        //}

        //public bool EndsWith(string value, StringComparison comparisonType)
        //{
        //    return this.Value.EndsWith(value, comparisonType);
        //}

        //public bool EndsWith(string value, bool ignoreCase, CultureInfo culture)
        //{
        //    return this.Value.EndsWith(value, ignoreCase, culture);
        //}

        //public bool Equals(string value, StringComparison comparisonType)
        //{
        //    return this.Value.Equals(value, comparisonType);
        //}

        // TODO: more methods.

        #endregion

        #region Implicit conversions

        /// <summary>
        /// Implicit conversion to string.
        /// </summary>
        public static implicit operator string(NonNullableString str)
        {
            return str.Value;
        }

        /// <summary>
        /// Implicit conversion from string.
        /// </summary>
        public static implicit operator NonNullableString(string str)
        {
            return new NonNullableString(str);
        }

        #endregion

        #region IConvertible support

        // These methods proxy System.String's IConvertible methods.
        // They are here primarily to allow NonNullableString to be
        // used with SqlParameterCollection.AddWithValue method.

        // Helper.
        private IConvertible AsConvertible()
        {
            return (IConvertible)this.Value;
        }

        // Proxies.
        TypeCode IConvertible.GetTypeCode()
        {
            return this.AsConvertible().GetTypeCode();
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return this.AsConvertible().ToBoolean(provider);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return this.AsConvertible().ToByte(provider);
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return this.AsConvertible().ToChar(provider);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return this.AsConvertible().ToDateTime(provider);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return this.AsConvertible().ToDecimal(provider);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return this.AsConvertible().ToDouble(provider);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return this.AsConvertible().ToInt16(provider);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return this.AsConvertible().ToInt32(provider);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return this.AsConvertible().ToInt64(provider);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return this.AsConvertible().ToSByte(provider);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return this.AsConvertible().ToSingle(provider);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return this.AsConvertible().ToString(provider);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return this.AsConvertible().ToType(conversionType, provider);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return this.AsConvertible().ToUInt16(provider);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return this.AsConvertible().ToUInt32(provider);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return this.AsConvertible().ToUInt64(provider);
        }

        #endregion
    }
}