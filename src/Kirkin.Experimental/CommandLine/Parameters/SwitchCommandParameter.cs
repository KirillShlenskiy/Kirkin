using System;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class SwitchCommandParameter
        : CommandParameterBase<bool>
    {
        internal SwitchCommandParameter(string name, string shortName)
            : base(name, shortName)
        {
        }

        public override bool ParseArgs(string[] args)
        {
            if (args == null) return false;

            if (args.Length > 1) {
                throw new InvalidOperationException($"Multiple argument values are not supported for switch '{Name}'.");
            }

            return args.Length == 0 // A switch does not need to have a value to be true.
                || Convert.ToBoolean(args[0]);
        }
    }
}