using System;
using System.Collections.Generic;

namespace Kirkin.Data
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

        public sealed class DataColumnLiteCollection : List<DataColumnLite>
        {
            private Dictionary<string, int> __columnNameToIndexMappings;

            internal Dictionary<string, int> ColumnNameToIndexMappings
            {
                get
                {
                    if (__columnNameToIndexMappings == null) {
                        RefreshColumnOrdinalMappings();
                    }

                    return __columnNameToIndexMappings;
                }
            }

            public DataColumnLite this[string name]
            {
                get
                {
                    return this[ColumnNameToIndexMappings[name]];
                }
            }

            internal DataColumnLiteCollection()
            {
            }

            /// <summary>
            /// Creates a new column with the given name and data type, and adds it to the table.
            /// </summary>
            public DataColumnLite Add(string name, Type dataType)
            {
                DataColumnLite column = new DataColumnLite(name, dataType);

                Add(column);

                // Invalidate mappings.
                __columnNameToIndexMappings = null;

                return column;
            }

            private void RefreshColumnOrdinalMappings()
            {
                Dictionary<string, int> dict = new Dictionary<string, int>(Count, StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < Count; i++) {
                    dict.Add(this[i].ColumnName, i);
                }

                __columnNameToIndexMappings = dict;
            }
        }

        public sealed class DataRowLiteCollection : List<DataRowLite>
        {
            private readonly DataTableLite Table;
            private int _capacity;

            internal DataRowLiteCollection(DataTableLite table)
            {
                Table = table;
            }

            /// <summary>
            /// Creates a new row from the given item array and adds it to the table.
            /// </summary>
            public DataRowLite Add(params object[] itemArray)
            {
                if (itemArray == null) throw new ArgumentNullException(nameof(itemArray));

                if (_capacity == Count)
                {
                    int newCapacity = (_capacity == 0) ? 16 : _capacity * 2;

                    foreach (DataColumnLite column in Table.Columns) {
                        column.Data.Capacity = newCapacity;
                    }

                    _capacity = newCapacity;
                }

                DataRowLite row = new DataRowLite(Table, Count, itemArray);

                base.Add(row);

                return row;
            }
        }
    }
}