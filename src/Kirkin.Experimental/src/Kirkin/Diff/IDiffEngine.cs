namespace Kirkin.Diff
{
    public interface IDiffEngine<T>
    {
        IDiffResult Compare(string name, T x, T y);
    }
}