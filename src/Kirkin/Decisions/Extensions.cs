using System;

using Kirkin.Decisions.Internal;

namespace Kirkin.Decisions
{
    public static class Extensions
    {
        public static IPreference<TInput> WithInputConversion<TInput, TConverted>(this IPreference<TConverted> preference, Func<TInput, TConverted> conversion)
        {
            return new ProjectPreference<TInput, TConverted>(preference.ToString(), conversion, preference);
        }

        public static IPreference<double> WithExponentialAdjustment(this IPreference<double> preference, double power)
        {
            return new ExponentialAdjustPreference(preference, power);
        }

        public static IPreference<double> WithMultiplier(this IPreference<double> preference, double multiplier)
        {
            return new MultiplyPreference(preference, multiplier);
        }
    }
}