using System;

namespace Kirkin.Data
{
    /// <summary>
    /// Lightweight DataRow-like data structure.
    /// </summary>
    public sealed class DataRowLite
    {
        private readonly int _rowIndex;

        /// <summary>
        /// Table that this row belongs to.
        /// </summary>
        public DataTableLite Table { get; }

        public object this[int index]
        {
            get
            {
                return Table.Columns[index].Data.Get(_rowIndex);
            }
            set
            {
                Table.Columns[index].Data.Set(_rowIndex, value);
            }
        }

        public object this[string name]
        {
            get
            {
                return Table.Columns[name].Data.Get(_rowIndex);
            }
            set
            {
                Table.Columns[name].Data.Set(_rowIndex, value);
            }
        }

        internal DataRowLite(DataTableLite table, int rowIndex)
        {
            Table = table;
            _rowIndex = rowIndex;
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
            IColumnData untypedData = column.Data;

            if (untypedData.IsNull(_rowIndex)) {
                throw new InvalidOperationException("The data is null.");
            }

            ColumnData<T> typedData = untypedData as ColumnData<T>;

            if (typedData != null) {
                return typedData.Get(_rowIndex);
            }

            return (T)untypedData.Get(_rowIndex);
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
            IColumnData untypedData = column.Data;

            if (untypedData.IsNull(_rowIndex)) {
                return default(T);
            }

            ColumnData<T> typedData = untypedData as ColumnData<T>;

            if (typedData != null) {
                return typedData.Get(_rowIndex);
            }

            return (T)untypedData.Get(_rowIndex);
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
            return column.Data.IsNull(_rowIndex);
        }
    }
}