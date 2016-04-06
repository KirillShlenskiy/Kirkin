namespace Kirkin.Diff
{
    public interface IDiffResult
    {
        bool AreSame { get; }
        string Message { get; }
    }
}