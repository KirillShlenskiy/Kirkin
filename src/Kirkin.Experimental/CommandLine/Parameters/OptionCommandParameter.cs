using System;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class OptionCommandParameter
        : CommandParameterBase<string>
    {
        internal OptionCommandParameter(string name, string shortName)
            : base(name, shortName)
        {
        }

        public override string ParseArgs(string[] args)
        {
            if (args == null || args.Length == 0) return null;

            if (args.Length > 1) {
                throw new InvalidOperationException($"Multiple argument values are not supported for option '{Name}'.");
            }

            return args[0];
        }
    }
}