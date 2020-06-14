namespace Kirkin.CommandLine
{
    internal interface IParseArgResult
    {
        object Value { get; }
        bool ExpectingMoreValues { get; }
    }
}
