using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class MainCommandParameter
        : CommandParameterBase<string>
    {
        public override bool SupportsMultipleValues
        {
            get
            {
                return false;
            }
        }

        internal MainCommandParameter(string name, string help)
            : base(name, null, true, help)
        {
        }

        public override ParseArgResult<string> ParseArgsImpl(List<string> args)
        {
            if (args.Count == 0) return new ParseArgResult<string>(null);
            if (args.Count > 1) throw new InvalidOperationException("Multiple parameter values are not supported.");

            return new ParseArgResult<string>(args[0]);
        }

        public override string ToString()
        {
            return $"<{Name}>";
        }
    }
}