using System.Collections.Generic;

using Kirkin.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class CommandParameterList
        : CommandParameterBase<string[]>, ICommandParameter
    {
        public override bool SupportsMultipleValues
        {
            get
            {
                return true;
            }
        }

        public CommandParameterList(string name, string help, bool isPositionalParameter)
            : base(name, null, help, isPositionalParameter)
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
            return $"<{Name} 1> <{Name} 2> ...";
        }
    }
}