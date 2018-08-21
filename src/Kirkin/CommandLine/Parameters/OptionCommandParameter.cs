using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class OptionCommandParameter
        : CommandParameterBase<string>, IParameterFormattable
    {
        public override bool SupportsMultipleValues
        {
            get
            {
                return false;
            }
        }

        internal OptionCommandParameter(string name, string shortName, bool isPositionalParameter, string help)
            : base(name, shortName, isPositionalParameter, help)
        {
        }

        public override string ParseArgsImpl(List<string> args)
        {
            // If the option is specified, we want a value
            // different from the default (which is null).
            if (args.Count == 0) return string.Empty;
            if (args.Count > 1) throw new InvalidOperationException($"Multiple argument values are not supported for option '{Name}'.");

            return args[0];
        }

        public override string ToString()
        {
            if (IsPositionalParameter)
            {
                return string.IsNullOrEmpty(ShortName)
                    ? $"[--{Name}] <arg>"
                    : $"[-{ShortName}|--{Name}] <arg>";
            }
            else
            {
                return string.IsNullOrEmpty(ShortName)
                    ? $"--{Name} <arg>"
                    : $"-{ShortName}|--{Name} <arg>";
            }
        }

        string IParameterFormattable.ToShortString()
        {
            if (IsPositionalParameter)
            {
                return !string.IsNullOrEmpty(ShortName)
                    ? $"[-{ShortName}] <arg>"
                    : $"[--{Name}] <arg>";
            }
            else
            {
                return !string.IsNullOrEmpty(ShortName)
                    ? $"-{ShortName} <arg>"
                    : $"--{Name} <arg>";
            }
        }

        string IParameterFormattable.ToLongString()
        {
            if (IsPositionalParameter)
            {
                return string.IsNullOrEmpty(ShortName)
                    ? $"[--{Name}] <arg>"
                    : $"[-{ShortName}, --{Name}] <arg>";
            }
            else
            {
                return string.IsNullOrEmpty(ShortName)
                    ? $"--{Name} <arg>"
                    : $"-{ShortName}, --{Name} <arg>";
            }
        }
    }
}