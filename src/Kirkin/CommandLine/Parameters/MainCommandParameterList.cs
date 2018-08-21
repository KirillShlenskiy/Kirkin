using System.Collections.Generic;

using Kirkin.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    internal sealed class MainCommandParameterList
        : CommandParameterBase<string[]>
    {
        public override bool SupportsMultipleValues
        {
            get
            {
                return true;
            }
        }

        public MainCommandParameterList(string name, string help)
            : base(name, null, true, help)
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
            return $"<{Name} 1> <{Name} 2> ...";
        }
    }
}