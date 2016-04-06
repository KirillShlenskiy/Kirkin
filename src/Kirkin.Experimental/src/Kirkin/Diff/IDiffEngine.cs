namespace Kirkin.Diff
{
    public interface IDiffEngine<T>
    {
        IDiffResult Compare(T x, T y);
    }
}