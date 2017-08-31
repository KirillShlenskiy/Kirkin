namespace Kirkin.Data.Internal
{
    /// <summary>
    /// Non-generic column data container contract.
    /// </summary>
    internal interface IColumnData
    {
        int Capacity { get; set; }

        bool IsNull(int index);
        object Get(int index);
        void Set(int index, object value);
        void Remove(int index);
    }
}