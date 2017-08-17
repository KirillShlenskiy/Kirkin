using System;
using System.Collections.Generic;

namespace Kirkin.Data
{
    public sealed class DataRowLiteCollection : List<DataRowLite>
    {
        private const int DEFAULT_CAPACITY = 16;

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
            if (itemArray.Length != Table.Columns.Count) throw new ArgumentException("Item array length/column number mismatch.");

            if (_capacity == Count)
            {
                int newCapacity = (_capacity == 0) ? DEFAULT_CAPACITY : _capacity * 2;

                foreach (DataColumnLite column in Table.Columns) {
                    column.Data.Capacity = newCapacity;
                }

                _capacity = newCapacity;
            }

            DataRowLite row = new DataRowLite(Table, Count);

            for (int i = 0; i < itemArray.Length; i++) {
                row[i] = itemArray[i];
            }

            base.Add(row);

            return row;
        }
    }
}