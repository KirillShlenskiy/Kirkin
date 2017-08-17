using System;

namespace Kirkin.Data
{
    /// <summary>
    /// Lightweight DataRow-like data structure.
    /// </summary>
    public sealed class DataRowLite
    {
        private readonly DataColumnLiteCollection _columns;
        private readonly int _rowIndex;

        /// <summary>
        /// Table that this row belongs to.
        /// </summary>
        public DataTableLite Table
        {
            get
            {
                return _columns.Table;
            }
        }

        /// <summary>
        /// Gets or sets the value of the cell at the specified column index.
        /// </summary>
        public object this[int columnIndex]
        {
            get
            {
                return this[_columns[columnIndex]];
            }
            set
            {
                this[_columns[columnIndex]] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the cell that belongs to the column with the specified name.
        /// </summary>
        public object this[string columnName]
        {
            get
            {
                return _columns.GetColumnData(columnName).Get(_rowIndex);
            }
            set
            {
                _columns.GetColumnData(columnName).Set(_rowIndex, value);
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
            _columns = table.Columns;
            _rowIndex = rowIndex;
        }

        /// <summary>
        /// Returns the value of the cell at the specified index.
        /// </summary>
        public T GetValue<T>(int columnIndex)
        {
            return GetValueImpl<T>(_columns[columnIndex].Data);
        }

        /// <summary>
        /// Returns the value of the cell that belongs to the column with the specified name.
        /// </summary>
        public T GetValue<T>(string columnName)
        {
            //return GetValue<T>(_columns[columnName]);
            return GetValueImpl<T>(_columns.GetColumnData(columnName));
        }

        /// <summary>
        /// Returns or sets the value of the cell that belongs to the specified column.
        /// </summary>
        public T GetValue<T>(DataColumnLite column)
        {
            return GetValueImpl<T>(column.Data);
        }

        private T GetValueImpl<T>(IColumnData data)
        {
            if (data.IsNull(_rowIndex)) throw new InvalidOperationException("The data is null.");
            if (data is ColumnData<T> typedData) return typedData.Get(_rowIndex);

            return (T)data.Get(_rowIndex);
        }

        /// <summary>
        /// Returns the value of the cell at the specified index, or default value for type if the value is null.
        /// </summary>
        public T GetValueOrDefault<T>(int columnIndex)
        {
            return GetValueOrDefaultImpl<T>(_columns[columnIndex].Data);
        }

        /// <summary>
        /// Returns the value of the cell that belongs to the column with the specified name, or default value for type if the value is null.
        /// </summary>
        public T GetValueOrDefault<T>(string columnName)
        {
            //return GetValueOrDefault<T>(_columns[columnName]);
            return GetValueOrDefaultImpl<T>(_columns.GetColumnData(columnName));
        }

        /// <summary>
        /// Returns or sets the value of the cell that belongs to the specified column, or default value for type if the value is null.
        /// </summary>
        public T GetValueOrDefault<T>(DataColumnLite column)
        {
            return GetValueOrDefaultImpl<T>(column.Data);
        }

        private T GetValueOrDefaultImpl<T>(IColumnData data)
        {
            if (data.IsNull(_rowIndex)) return default(T);
            if (data is ColumnData<T> typedData) return typedData.Get(_rowIndex);

            return (T)data.Get(_rowIndex);
        }

        /// <summary>
        /// Returns true if the value of the cell at the specified index is null.
        /// </summary>
        public bool IsNull(int columnIndex)
        {
            return IsNullImpl(_columns[columnIndex].Data);
        }

        /// <summary>
        /// Returns true if the value of the cell that belongs to the column with the specified name is null.
        /// </summary>
        public bool IsNull(string columnName)
        {
            //return IsNull(_columns[columnName]);
            return IsNullImpl(_columns.GetColumnData(columnName));
        }

        /// <summary>
        /// Returns true if the value of the cell that belongs to the specified column is null.
        /// </summary>
        public bool IsNull(DataColumnLite column)
        {
            return IsNullImpl(column.Data);
        }

        private bool IsNullImpl(IColumnData data)
        {
            return data.IsNull(_rowIndex);
        }
    }
}