﻿namespace Kirkin.Data
{
    /// <summary>
    /// Lightweight DataColumn-like data structure.
    /// </summary>
    public sealed class DataTableLite
    {
        /// <summary>
        /// Collection of column definitions in this table.
        /// </summary>
        public DataColumnLiteCollection Columns { get; }

        /// <summary>
        /// Collection of rows that belong to this table.
        /// </summary>
        public DataRowLiteCollection Rows { get; }

        /// <summary>
        /// Creates a new <see cref="DataTableLite"/> instance.
        /// </summary>
        public DataTableLite()
        {
            Columns = new DataColumnLiteCollection();
            Rows = new DataRowLiteCollection(this);
        }
    }
}