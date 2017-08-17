using System;

namespace Kirkin.Data
{
    /// <summary>
    /// Lightweight DataRow-like data structure.
    /// </summary>
    public struct DataRowLite
    {
        private readonly int _rowIndex;

        /// <summary>
        /// Table that this row belongs to.
        /// </summary>
        public DataTableLite Table { get; }

        /// <summary>
        /// Gets or sets the value of the cell at the specified column index.
        /// </summary>
        public object this[int columnIndex]
        {
            get
            {
                return this[Table.Columns[columnIndex]];
            }
            set
            {
                this[Table.Columns[columnIndex]] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the cell that belongs to the column with the specified name.
        /// </summary>
        public object this[string columnName]
        {
            get
            {
                return this[Table.Columns[columnName]];
            }
            set
            {
                this[Table.Columns[columnName]] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the cell that belongs to the specified column.
        /// </summary>
        public object this[DataColumnLite column]
        {
            get
            {
                return column.Data.Get(_rowIndex);
            }
            set
            {
                column.Data.Set(_rowIndex, value);
            }
        }

        internal DataRowLite(DataTableLite table, int rowIndex)
        {
            Table = table;
            _rowIndex = rowIndex;
        }

        /// <summary>
        /// Gets the value of the cell at the specified index.
        /// </summary>
        public T GetValue<T>(int columnIndex)
        {
            return GetValue<T>(Table.Columns[columnIndex]);
        }

        public T GetValue<T>(string columnName)
        {
            return GetValue<T>(Table.Columns[columnName]);
        }

        public T GetValue<T>(DataColumnLite column)
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

        public T GetValueOrDefault<T>(int columnIndex)
        {
            return GetValueOrDefault<T>(Table.Columns[columnIndex]);
        }

        public T GetValueOrDefault<T>(string columnName)
        {
            return GetValueOrDefault<T>(Table.Columns[columnName]);
        }

        public T GetValueOrDefault<T>(DataColumnLite column)
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

        public bool IsNull(int columnIndex)
        {
            return IsNull(Table.Columns[columnIndex]);
        }

        public bool IsNull(string columnName)
        {
            return IsNull(Table.Columns[columnName]);
        }

        public bool IsNull(DataColumnLite column)
        {
            return column.Data.IsNull(_rowIndex);
        }
    }
}