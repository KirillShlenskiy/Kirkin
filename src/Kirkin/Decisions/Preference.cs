using Kirkin.Decisions.Internal;

namespace Kirkin.Decisions
{
    /// <summary>
    /// <see cref="IPreference{TInput}"/> factory type.
    /// </summary>
    public static class Preference
    {
        public static IPreference<TInput> Combine<TInput>(string name, params IPreference<TInput>[] preferences)
        {
            return new CompositePreference<TInput>(name, preferences);
        }

        public static IPreference<double> HigherIsBetter(double minValue, double maxValue)
        {
            return new HigherIsBetterPreference(minValue, maxValue);
        }

        public static IPreference<double> LowerIsBetter(double minValue, double maxValue)
        {
            return new LowerIsBetterPreference(minValue, maxValue);
        }
    }
}