using System;
using System.Threading;

namespace Kirkin
{
    /// <summary>
    /// 64-bit data type used by timestamp columns.
    /// </summary>
    [Serializable]
    public struct Timestamp : IEquatable<Timestamp>, IComparable<Timestamp>
    {
        /// <summary>
        /// Underlying timestamp value.
        /// </summary>
        private long Value;

        /// <summary>
        /// Creates a timestamp with the given value.
        /// </summary>
        public Timestamp(ulong value)
        {
            Value = unchecked((long)value);
        }

        /// <summary>
        /// Creates a timestamp with the given value.
        /// </summary>
        public Timestamp(long value)
        {
            Value = value;
        }

        /// <summary>
        /// Creates a timestamp from the given byte array.
        /// </summary>
        public Timestamp(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (bytes.Length > 8) throw new ArgumentException("Arrays longer than 8 bytes not supported.");

            // Normalize bytes to an 8-byte array.
            if (bytes.Length < 8)
            {
                byte[] newBytes = new byte[8];

                Array.Copy(bytes, 0, newBytes, newBytes.Length - bytes.Length, bytes.Length);

                bytes = newBytes;
            }

            int i1 = (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
            int i2 = (bytes[4] << 24) | (bytes[5] << 16) | (bytes[6] << 8) | bytes[7];
            ulong unsigned = (uint)i2 | ((ulong)i1 << 32);

            Value = unchecked((long)unsigned);
        }

        /// <summary>
        /// Parses the given timestamp value. Can handle values starting with "0x" and values containing hyphens.
        /// </summary>
        public static Timestamp Parse(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Text cannot be empty."); // In line with framework's standard Parse methods.

            // Rebuild string.
            char[] chars = new char[text.Length];
            int length = 0;

            // Remove "0x" from start.
            int startIndex = (text.Length >= 2 && text[0] == '0' && text[1] == 'x') ? 2 : 0;

            for (int i = startIndex; i < text.Length; i++)
            {
                char c = text[i];

                // Remove "-".
                if (c != '-') {
                    chars[length++] = c;
                }
            }

            text = new string(chars, 0, length);

            // Convert.
            long value = Convert.ToInt64(text, 16);

            return new Timestamp(value);
        }

        /// <summary>
        /// Parses the given timestamp value. Can handle values starting with "0x",
        /// but throws a <see cref="FormatException"/> if the text contains hyphens.
        /// </summary>
        public static Timestamp ParseExact(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            long value = Convert.ToInt64(text, 16);

            return new Timestamp(value);
        }

        /// <summary>
        /// Compares this instance to the specified timestamp
        /// and returns an indicator of their relative values.
        /// </summary>
        public int CompareTo(Timestamp other)
        {
            return ToUInt64().CompareTo(other.ToUInt64());
        }

        /// <summary>
        /// Returns a value indicating whether this value
        /// is equal to the specified timestamp value.
        /// </summary>
        public bool Equals(Timestamp other)
        {
            return Value == other.Value;
        }

        /// <summary>
        /// Returns a value indicating whether this
        /// value is equal to the specified object.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Timestamp && Equals((Timestamp)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// Returns the value of this instance as a big-endian byte array.
        /// </summary>
        public byte[] ToArray()
        {
            long value = Value;
            byte[] bytes = new byte[8];
            int int1 = (int)(value >> 32);
            int int2 = (int)(value & uint.MaxValue);

            for (int i = 0; i < 4; i++)
            {
                bytes[3 - i] = (byte)(int1 >> (i * 8));
                bytes[7 - i] = (byte)(int2 >> (i * 8));
            }

            return bytes;
        }

        /// <summary>
        /// Returns the value of this instance as a signed 64-bit integer.
        /// </summary>
        public long ToInt64()
        {
            return Value;
        }

        /// <summary>
        /// Returns the value of this instance as an unsigned 64-bit integer.
        /// </summary>
        public ulong ToUInt64()
        {
            return unchecked((ulong)Value);
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            // Prevent torn reads by dereferencing "this" only once.
            // The meaning of "this" can change during this method's
            // execution if the field or variable containing this
            // value is reassigned, resulting in a torn read.
            ulong value = ToUInt64();

            return value == 0 ? "" : value.ToString("X");
        }

        #region Interlocked operations

        // TODO: InterlockedRead, InterlockedAdd.

        /// <summary>
        /// Sets a <see cref="Timestamp"/> to the specified value and returns the original value, as an atomic operation.
        /// </summary>
        /// <returns>The original value in <param name="location"/></returns>
        public static Timestamp InterlockedExchange(ref Timestamp location, Timestamp newValue)
        {
            return new Timestamp(Interlocked.Exchange(ref location.Value, newValue.Value));
        }

        /// <summary>
        /// Compares two <see cref="Timestamp"/> values for equality and, if they are equal, replaces the first value.
        /// </summary>
        /// <returns>The original value in <param name="location"/></returns>
        public static Timestamp InterlockedCompareExchange(ref Timestamp location, Timestamp newValue, Timestamp comparand)
        {
            return new Timestamp(Interlocked.CompareExchange(ref location.Value, newValue.Value, comparand.Value));
        }

        /// <summary>
        /// Increments the specified <see cref="Timestamp"/> variable and stores the result, as an atomic operation.
        /// </summary>
        public static Timestamp InterlockedIncrement(ref Timestamp location)
        {
            return new Timestamp(Interlocked.Increment(ref location.Value));
        }

        /// <summary>
        /// Decrements the specified <see cref="Timestamp"/> variable and stores the result, as an atomic operation.
        /// </summary>
        public static Timestamp InterlockedDecrement(ref Timestamp location)
        {
            return new Timestamp(Interlocked.Decrement(ref location.Value));
        }

        #endregion

        #region Volatile operations

        // TODO: Remove?

        /// <summary>
        /// Reads the value of the specified field. On systems that require it, inserts a
        //  memory barrier that prevents the processor from reordering memory operations
        //  as follows: If a read or write appears after this method in the code, the processor
        //  cannot move it before this method.
        /// </summary>
        internal static Timestamp VolatileRead(ref Timestamp location)
        {
            return new Timestamp(Volatile.Read(ref location.Value));
        }

        /// <summary>
        /// Writes the specified value to the specified field. On systems that require it,
        //  inserts a memory barrier that prevents the processor from reordering memory operations
        //  as follows: If a memory operation appears before this method in the code, the
        //  processor cannot move it after this method.
        /// </summary>
        internal static void VolatileWrite(ref Timestamp location, Timestamp value)
        {
            Volatile.Write(ref location.Value, value.Value);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Returns the value of this instance as a signed 64-bit integer.
        /// </summary>
        public static explicit operator long(Timestamp timestamp)
        {
            return timestamp.Value;
        }

        /// <summary>
        /// Returns the value of this instance as an unsigned 64-bit integer.
        /// </summary>
        public static explicit operator ulong(Timestamp timestamp)
        {
            return unchecked((ulong)timestamp.Value);
        }

        /// <summary>
        /// Checks the two <see cref="Timestamp"/> values for equality.
        /// </summary>
        public static bool operator ==(Timestamp x, Timestamp y)
        {
            return x.Value == y.Value;
        }

        /// <summary>
        /// Checks the two <see cref="Timestamp"/> values for inequality.
        /// </summary>
        public static bool operator !=(Timestamp x, Timestamp y)
        {
            return x.Value != y.Value;
        }

        // Deliberately ambiguous overload to prevent
        // unpredictable null compare behaviour.
        public static bool operator ==(Timestamp x, object y)
        {
            return Equals(x, y);
        }

        // Deliberately ambiguous overload to prevent
        // unpredictable null compare behaviour.
        public static bool operator !=(Timestamp x, object y)
        {
            return !Equals(x, y);
        }

        /// <summary>
        /// Returns true if the LHS <see cref="Timestamp"/> value is greater than RHS value.
        /// </summary>
        public static bool operator >(Timestamp x, Timestamp y)
        {
            return x.ToUInt64() > y.ToUInt64();
        }

        /// <summary>
        /// Returns true if the LHS <see cref="Timestamp"/> value is smaller than RHS value.
        /// </summary>
        public static bool operator <(Timestamp x, Timestamp y)
        {
            return x.ToUInt64() < y.ToUInt64();
        }

        /// <summary>
        /// Performs a non-atomic increment of the given <see cref="Timestamp"/> value.
        /// </summary>
        public static Timestamp operator ++(Timestamp timestamp)
        {
            return new Timestamp(timestamp.ToUInt64() + 1);
        }

        /// <summary>
        /// Performs a non-atomic decrement of the given <see cref="Timestamp"/> value.
        /// </summary>
        public static Timestamp operator --(Timestamp timestamp)
        {
            return new Timestamp(timestamp.ToUInt64() - 1);
        }

        #endregion
    }
}