using System;

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

        public T GetValue<T>(int index)
        {
            return GetValue<T>(Table.Columns[index]);
        }

        public T GetValue<T>(string name)
        {
            return GetValue<T>(Table.Columns[name]);
        }

        private T GetValue<T>(DataColumnLite column)
        {
            IColumnStorage untypedData = column.Data;

            if (untypedData.IsNull(RowIndex)) {
                throw new InvalidOperationException("The data is null.");
            }

            ColumnStorage<T> typedData = untypedData as ColumnStorage<T>;

            if (typedData != null) {
                return typedData.Get(RowIndex);
            }

            return (T)untypedData.Get(RowIndex);
        }

        public T GetValueOrDefault<T>(int index)
        {
            return GetValueOrDefault<T>(Table.Columns[index]);
        }

        public T GetValueOrDefault<T>(string name)
        {
            return GetValueOrDefault<T>(Table.Columns[name]);
        }

        private T GetValueOrDefault<T>(DataColumnLite column)
        {
            IColumnStorage untypedData = column.Data;

            if (untypedData.IsNull(RowIndex)) {
                return default(T);
            }

            ColumnStorage<T> typedData = untypedData as ColumnStorage<T>;

            if (typedData != null) {
                return typedData.Get(RowIndex);
            }

            return (T)untypedData.Get(RowIndex);
        }

        public bool IsNull(int index)
        {
            return IsNull(Table.Columns[index]);
        }

        public bool IsNull(string name)
        {
            return IsNull(Table.Columns[name]);
        }

        private bool IsNull(DataColumnLite column)
        {
            return column.Data.IsNull(RowIndex);
        }
    }
}