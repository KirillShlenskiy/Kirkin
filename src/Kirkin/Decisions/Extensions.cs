using System;

namespace Kirkin.Decisions
{
    public static class Extensions
    {
        public static IPreference<TInput> WithInputConversion<TInput, TConverted>(this IPreference<TConverted> preference, Func<TInput, TConverted> conversion)
        {
            return new ProjectPreference<TInput, TConverted>(preference.ToString(), conversion, preference);
        }
    }
}