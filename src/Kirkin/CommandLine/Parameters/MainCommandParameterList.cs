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

        public override ParseArgResult<string[]> ParseArgsImpl(List<string> args)
        {
            if (args.Count == 0) return new ParseArgResult<string[]>(GetDefaultValueImpl());

            return new ParseArgResult<string[]>(args.ToArray());
        }

        public override string ToString()
        {
            return $"<{Name}>...";
        }
    }
}