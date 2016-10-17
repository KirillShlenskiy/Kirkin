namespace Kirkin.Refs
{
    public interface IRef
    {
        object Value { get; set; }
    }

    public interface IRef<T> : IRef
    {
        new T Value { get; set; }
    }
}