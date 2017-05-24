using System;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class CommandParameter
        : CommandParameterBase<string>
    {
        internal CommandParameter(string name)
            : base(name, null)
        {
        }

        public override string ParseArgs(string[] args)
        {
            if (args == null || args.Length == 0) return null;

            if (args.Length > 1) {
                throw new InvalidOperationException("Multiple parameter values are not supported.");
            }

            return args[0];
        }
    }
}