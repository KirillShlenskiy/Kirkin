using System;
using System.Collections;
using System.Collections.Generic;

using Kirkin.Collections.Generic.Enumerators;

namespace Kirkin.Data
{
    public sealed class DataRowLiteCollection : IEnumerable<DataRowLite>
    {
        private const int DEFAULT_CAPACITY = 16;

        private readonly DataTableLite Table;
        private List<DataRowLite> _rows = new List<DataRowLite>(DEFAULT_CAPACITY);
        private int _capacity;

        internal DataRowLiteCollection(DataTableLite table)
        {
            Table = table;
        }

        public DataRowLite this[int index]
        {
            get
            {
                return _rows[index];
            }
        }

        public int Count
        {
            get
            {
                return _rows.Count;
            }
        }

        /// <summary>
        /// Creates a new row filled with nulls.
        /// </summary>
        public DataRowLite AddNewRow()
        {
            EnsureSufficientCapacityForAdd();

            DataRowLite row = new DataRowLite(Table, Count);

            _rows.Add(row);

            return row;
        }

        /// <summary>
        /// Creates a new row from the given item array and adds it to the table.
        /// </summary>
        public DataRowLite Add(params object[] itemArray)
        {
            if (itemArray == null) throw new ArgumentNullException(nameof(itemArray));
            if (itemArray.Length != Table.Columns.Count) throw new ArgumentException("Item array length/column number mismatch.");

            EnsureSufficientCapacityForAdd();

            DataRowLite row = new DataRowLite(Table, Count);

            for (int i = 0; i < itemArray.Length; i++) {
                row[i] = itemArray[i];
            }

            _rows.Add(row);

            return row;
        }

        private void EnsureSufficientCapacityForAdd()
        {
            if (_rows.Count == _capacity)
            {
                int newCapacity = (_capacity == 0) ? DEFAULT_CAPACITY : _capacity * 2;

                foreach (DataColumnLite column in Table.Columns) {
                    column.Data.Capacity = newCapacity;
                }

                _capacity = newCapacity;
            }
        }

        public void Clear()
        {
            _rows.Clear();

            foreach (DataColumnLite column in Table.Columns) {
                column.Data.Capacity = 0;
            }
        }

        internal void TrimExcess()
        {
            _rows.TrimExcess();

            foreach (DataColumnLite column in Table.Columns) {
                column.Data.Capacity = _capacity;
            }
        }

        public ListEnumerator<DataRowLite> GetEnumerator()
        {
            return new ListEnumerator<DataRowLite>(_rows);
        }

        IEnumerator<DataRowLite> IEnumerable<DataRowLite>.GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rows.GetEnumerator();
        }
    }
}