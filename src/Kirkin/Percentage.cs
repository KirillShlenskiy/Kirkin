using System;

namespace Kirkin
{
    /// <summary>
    /// Facilitates percentage calculation.
    /// </summary>
    public static class Percentage
    {
        /// <summary>
        /// Calculates the percentage based on the given position and total figures.
        /// </summary>
        public static int Calculate(long current, long max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
            if (current < 0) throw new ArgumentOutOfRangeException(nameof(current));
            if (current > max) throw new ArgumentException("Current cannot be greater than Max.");

            int result = (int)((double)current / max * 100.0);

            return Math.Min(result, 100);
        }
    }
}