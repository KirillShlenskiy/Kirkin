using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class CommandParameter
        : CommandParameterBase<string>
    {
        public override bool SupportsMultipleValues
        {
            get
            {
                return false;
            }
        }

        internal CommandParameter(string name)
            : base(name, null)
        {
        }

        public override string ParseArgs(List<string> args)
        {
            if (args.Count == 0) return null;
            if (args.Count > 1) throw new InvalidOperationException("Multiple parameter values are not supported.");

            return args[0];
        }

        public override string ToString()
        {
            return $"<{Name}>";
        }
    }
}