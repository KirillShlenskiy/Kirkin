﻿using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class SwitchCommandParameter
        : CommandParameterBase<bool>
    {
        internal SwitchCommandParameter(string name, string shortName)
            : base(name, shortName)
        {
        }

        public override bool ParseArgs(List<string> args)
        {
            if (args.Count > 1) throw new InvalidOperationException($"Multiple argument values are not supported for switch '{Name}'.");

            return args.Count == 0 // A switch does not need to have a value to be true.
                || Convert.ToBoolean(args[0]);
        }
    }
}