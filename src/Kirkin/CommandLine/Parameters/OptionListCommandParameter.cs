using System.Collections.Generic;

using Kirkin.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class OptionListCommandParameter
        : CommandParameterBase<string[]>, IParameterFormattable
    {
        public override bool SupportsMultipleValues
        {
            get
            {
                return true;
            }
        }

        internal OptionListCommandParameter(string name, string shortName, string help, bool isPositionalParameter)
            : base(name, shortName, help, isPositionalParameter)
        {
        }

        public override string[] GetDefaultValue()
        {
            return Array<string>.Empty;
        }

        public override string[] ParseArgs(List<string> args)
        {
            if (args.Count == 0) return GetDefaultValue();

            return args.ToArray();
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(ShortName)
                ? $"--{Name} <arg 1> <arg 2> ..."
                : $"-{ShortName}|--{Name} <arg 1> <arg 2> ...";
        }

        string IParameterFormattable.ToShortString()
        {
            return !string.IsNullOrEmpty(ShortName)
                ? $"-{ShortName} <arg 1> <arg 2> ..."
                : $"--{Name} <arg 1> <arg 2> ...";
        }

        string IParameterFormattable.ToLongString()
        {
            return string.IsNullOrEmpty(ShortName)
                ? $"--{Name} <arg 1> <arg 2> ..."
                : $"-{ShortName}, --{Name} <arg 1> <arg 2> ...";
        }
    }
}