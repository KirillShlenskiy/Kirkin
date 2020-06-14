namespace Kirkin.CommandLine
{
    internal class ParseArgResult<TValue> : IParseArgResult
    {
        public TValue Value { get; }
        public bool ExpectingMoreValues { get; }

        object IParseArgResult.Value => Value;

        public ParseArgResult(TValue value, bool expectingMoreValues = false)
        {
            Value = value;
            ExpectingMoreValues = expectingMoreValues;
        }
    }
}
