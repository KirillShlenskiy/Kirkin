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

        internal OptionListCommandParameter(string name, string shortName, bool isPositionalParameter, string help)
            : base(name, shortName, isPositionalParameter, help)
        {
        }

        public override string[] GetDefaultValueImpl()
        {
            return Array<string>.Empty;
        }

        public override string[] ParseArgsImpl(List<string> args)
        {
            if (args.Count == 0) return GetDefaultValueImpl();

            return args.ToArray();
        }

        public override string ToString()
        {
            if (IsPositionalParameter)
            {
                return string.IsNullOrEmpty(ShortName)
                    ? $"[--{Name}] <arg>..."
                    : $"[-{ShortName}|--{Name}] <arg>...";
            }
            else
            {
                return string.IsNullOrEmpty(ShortName)
                    ? $"--{Name} <arg>..."
                    : $"-{ShortName}|--{Name} <arg>...";
            }
        }

        string IParameterFormattable.ToShortString()
        {
            if (IsPositionalParameter)
            {
                return !string.IsNullOrEmpty(ShortName)
                    ? $"[-{ShortName}] <arg>..."
                    : $"[--{Name}] <arg>...";
            }
            else
            {
                return !string.IsNullOrEmpty(ShortName)
                    ? $"-{ShortName} <arg>..."
                    : $"--{Name} <arg>...";
            }
        }

        string IParameterFormattable.ToLongString()
        {
            if (IsPositionalParameter)
            {
                return string.IsNullOrEmpty(ShortName)
                    ? $"[--{Name}] <arg>..."
                    : $"[-{ShortName}, --{Name}] <arg>...";
            }
            else
            {
                return string.IsNullOrEmpty(ShortName)
                    ? $"--{Name} <arg>..."
                    : $"-{ShortName}, --{Name} <arg>...";
            }
            
        }
    }
}