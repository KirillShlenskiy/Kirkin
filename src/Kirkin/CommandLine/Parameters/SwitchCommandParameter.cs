using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class SwitchCommandParameter
        : CommandParameterBase<bool>, IParameterFormattable
    {
        public override bool SupportsMultipleValues
        {
            get
            {
                return false;
            }
        }

        internal SwitchCommandParameter(string name, string shortName, string help)
            : base(name, shortName, false, help)
        {
        }

        public override bool ParseArgs(List<string> args)
        {
            if (args.Count > 1) throw new InvalidOperationException($"Multiple argument values are not supported for switch '{Name}'.");

            return args.Count == 0 // A switch does not need to have a value to be true.
                || Convert.ToBoolean(args[0]);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(ShortName)
                ? $"--{Name}"
                : $"-{ShortName}|--{Name}";
        }

        string IParameterFormattable.ToShortString()
        {
            return !string.IsNullOrEmpty(ShortName)
                ? $"-{ShortName}"
                : $"--{Name}";
        }

        string IParameterFormattable.ToLongString()
        {
            return string.IsNullOrEmpty(ShortName)
                ? $"--{Name}"
                : $"-{ShortName}, --{Name}";
        }
    }
}