namespace Kirkin.Data
{
    internal interface IColumnStorage
    {
        int Capacity { get; set; }

        bool IsNull(int index);
        object Get(int index);
        void Set(int index, object value);
    }
}