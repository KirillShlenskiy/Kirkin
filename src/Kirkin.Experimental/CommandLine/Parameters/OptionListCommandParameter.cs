﻿using System.Collections.Generic;

using Kirkin.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class OptionListCommandParameter
        : CommandParameterBase<string[]>
    {
        public override bool SupportsMultipleValues
        {
            get
            {
                return true;
            }
        }

        internal OptionListCommandParameter(string name, string shortName, string help)
            : base(name, shortName, help)
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
    }
}