using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine
{
    internal static class CommandSyntax
    {
        /// <summary>
        /// Returns true if the given token is recognized as a help switch.
        /// </summary>
        internal static bool IsHelpSwitch(string arg, IEqualityComparer<string> equalityComparer)
        {
            return equalityComparer.Equals(arg, "--help")
                || equalityComparer.Equals(arg, "-?")
                || equalityComparer.Equals(arg, "/?");
        }

        /// <summary>
        /// Throws if the given term is a reserved keyword.
        /// </summary>
        internal static void EnsureNotAReservedKeyword(string term)
        {
            // For the purpose of validation we'll be more restrictive
            // and disallow reserved terms regardless of case.
            if (IsHelpSwitch(term, StringComparer.OrdinalIgnoreCase)) {
                throw new InvalidOperationException($"Reserved term: '{term}'.");
            }
        }
    }
}