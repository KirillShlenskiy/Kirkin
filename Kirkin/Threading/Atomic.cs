using System;
using System.Threading;

namespace Kirkin.Threading
{
    /// <summary>
    /// Additional Interlocked methods.
    /// </summary>
    public static class Atomic
    {
        // Most of these transformation methods only work in really
        // simple scenarios where the transformation is irreversible
        // and its results predictable, unique and consistent over time,
        // i.e. incrementing a variable. If it is possible that the transformation
        // applied to the same value at different points in time will produce
        // different results, you *will* corrupt the state of your application
        // (Google "ABA problem").

        /// <summary>
        /// Jeffrey Richter's "interlocked anything" pattern
        /// implementation shamelessly stolen from his book.
        /// </summary>
        public static int Transform<TArgument>(ref int target, TArgument argument, Func<int, TArgument, int> transformation)
        {
            int currentVal = target, startVal, desiredVal;

            do
            {
                startVal = currentVal;
                desiredVal = transformation(startVal, argument);
                currentVal = Interlocked.CompareExchange(ref target, desiredVal, startVal);
            }
            while (startVal != currentVal);

            return desiredVal;
        }

        /// <summary>
        /// Jeffrey Richter's "interlocked anything" pattern
        /// implementation shamelessly stolen from his book.
        /// In performance-critical scenarios you should
        /// consider using the other overload.
        /// </summary>
        public static int Transform(ref int target, Func<int, int> transformation)
        {
            int currentVal = target, startVal, desiredVal;

            do
            {
                startVal = currentVal;
                desiredVal = transformation(startVal);
                currentVal = Interlocked.CompareExchange(ref target, desiredVal, startVal);
            }
            while (startVal != currentVal);

            return desiredVal;
        }

        /// <summary>
        /// Jeffrey Richter's "interlocked anything" pattern
        /// implementation shamelessly stolen from his book.
        /// </summary>
        public static long Transform<TArgument>(ref long target, TArgument argument, Func<long, TArgument, long> transformation)
        {
            long currentVal = target, startVal, desiredVal;

            do
            {
                startVal = currentVal;
                desiredVal = transformation(startVal, argument);
                currentVal = Interlocked.CompareExchange(ref target, desiredVal, startVal);
            }
            while (startVal != currentVal);

            return desiredVal;
        }

        /// <summary>
        /// Jeffrey Richter's "interlocked anything" pattern
        /// implementation shamelessly stolen from his book.
        /// In performance-critical scenarios you should
        /// consider using the other overload.
        /// </summary>
        public static long Transform(ref long target, Func<long, long> transformation)
        {
            long currentVal = target, startVal, desiredVal;

            do
            {
                startVal = currentVal;
                desiredVal = transformation(startVal);
                currentVal = Interlocked.CompareExchange(ref target, desiredVal, startVal);
            }
            while (startVal != currentVal);

            return desiredVal;
        }

        /// <summary>
        /// Jeffrey Richter's "interlocked anything" pattern
        /// implementation shamelessly stolen from his book.
        /// </summary>
        public static T Transform<T, TArgument>(ref T target, TArgument argument, Func<T, TArgument, T> transformation)
            where T : class
        {
            T currentVal = target, startVal, desiredVal;

            do
            {
                startVal = currentVal;
                desiredVal = transformation(startVal, argument);
                currentVal = Interlocked.CompareExchange(ref target, desiredVal, startVal);
            }
            while (startVal != currentVal);

            return desiredVal;
        }

        /// <summary>
        /// Jeffrey Richter's "interlocked anything" pattern
        /// implementation shamelessly stolen from his book.
        /// In performance-critical scenarios you should
        /// consider using the other overload.
        /// </summary>
        public static bool TryTransform<T, TArgument>(ref T target, TArgument argument, Func<T, TArgument, T> transformation, out T desiredVal)
            where T : class
        {
            var startVal = target;

            desiredVal = transformation(startVal, argument);

            if (startVal == Interlocked.CompareExchange(ref target, desiredVal, startVal))
            {
                // Success.
                return true;
            }

            // Reset.
            desiredVal = default(T);

            return false;
        }

        /// <summary>
        /// Jeffrey Richter's "interlocked anything" pattern
        /// implementation shamelessly stolen from his book.
        /// Spins if the first CompareExchange is preempted.
        /// </summary>
        public static T SpinTransform<T, TArgument>(ref T target, TArgument argument, Func<T, TArgument, T> transformation)
            where T : class
        {
            T currentVal = target, startVal, desiredVal;

            // SpinWait is a tiny value type (1 int field),
            // so it doesn't hurt to allocate a spinner
            // even if we don't get to use it.
            var spinner = new SpinWait();

            do
            {
                startVal = currentVal;
                desiredVal = transformation(startVal, argument);
                currentVal = Interlocked.CompareExchange(ref target, desiredVal, startVal);

                if (startVal == currentVal)
                {
                    return desiredVal;
                }

                spinner.SpinOnce();
            }
            while (true);
        }

        /// <summary>
        /// Jeffrey Richter's "interlocked anything" pattern
        /// implementation shamelessly stolen from his book.
        /// In performance-critical scenarios you should
        /// consider using the other overload.
        /// </summary>
        public static T Transform<T>(ref T target, Func<T, T> transformation)
            where T : class
        {
            T currentVal = target, startVal, desiredVal;

            do
            {
                startVal = currentVal;
                desiredVal = transformation(startVal);
                currentVal = Interlocked.CompareExchange(ref target, desiredVal, startVal);
            }
            while (startVal != currentVal);

            return desiredVal;
        }

        /// <summary>
        /// Jeffrey Richter's "interlocked anything" pattern
        /// implementation shamelessly stolen from his book.
        /// In performance-critical scenarios you should
        /// consider using the other overload.
        /// </summary>
        public static bool TryTransform<T>(ref T target, Func<T, T> transformation, out T desiredVal)
            where T : class
        {
            var startVal = target;

            desiredVal = transformation(startVal);

            if (startVal == Interlocked.CompareExchange(ref target, desiredVal, startVal))
            {
                // Success.
                return true;
            }

            // Reset.
            desiredVal = default(T);

            return false;
        }

        /// <summary>
        /// Jeffrey Richter's "interlocked anything" pattern
        /// implementation shamelessly stolen from his book.
        /// Spins if the first CompareExchange is preempted.
        /// In performance-critical scenarios you should
        /// consider using the other overload.
        /// </summary>
        public static T SpinTransform<T>(ref T target, Func<T, T> transformation)
            where T : class
        {
            T currentVal = target, startVal, desiredVal;

            // SpinWait is a tiny value type (1 int field),
            // so it doesn't hurt to allocate a spinner
            // even if we don't get to use it.
            var spinner = new SpinWait();

            do
            {
                startVal = currentVal;
                desiredVal = transformation(startVal);
                currentVal = Interlocked.CompareExchange(ref target, desiredVal, startVal);

                if (startVal == currentVal)
                {
                    return desiredVal;
                }

                spinner.SpinOnce();
            }
            while (true);
        }
    }
}