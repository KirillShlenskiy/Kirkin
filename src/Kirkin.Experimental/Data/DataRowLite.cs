namespace Kirkin.Data
{
    /// <summary>
    /// Lightweight DataRow-like data structure.
    /// </summary>
    public sealed class DataRowLite
    {
        /// <summary>
        /// Table that this row belongs to.
        /// </summary>
        public DataTableLite Table { get; }

        internal int RowIndex { get; }

        internal object[] ItemArray
        {
            get
            {
                object[] arr = new object[Table.Columns.Count];

                for (int i = 0; i < arr.Length; i++) {
                    arr[i] = Table.Columns[i].Data.Get(RowIndex);
                }

                return arr;
            }
        }

        public object this[int index]
        {
            get
            {
                return Table.Columns[index].Data.Get(RowIndex);
            }
            set
            {
                Table.Columns[index].Data.Set(RowIndex, value);
            }
        }

        public object this[string name]
        {
            get
            {
                return Table.Columns[name].Data.Get(RowIndex);
            }
            set
            {
                Table.Columns[name].Data.Set(RowIndex, value);
            }
        }

        internal DataRowLite(DataTableLite table, int rowIndex, object[] itemArray)
        {
            Table = table;
            RowIndex = rowIndex;

            for (int i = 0; i < itemArray.Length; i++) {
                this[i] = itemArray[i];
            }
        }
    }
}