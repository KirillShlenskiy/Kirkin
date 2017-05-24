using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class OptionCommandParameter
        : CommandParameterBase<string>
    {
        internal OptionCommandParameter(string name, string shortName)
            : base(name, shortName)
        {
        }

        public override string ParseArgs(List<string> args)
        {
            if (args.Count == 0) return null;
            if (args.Count > 1) throw new InvalidOperationException($"Multiple argument values are not supported for option '{Name}'.");

            return args[0];
        }
    }
}