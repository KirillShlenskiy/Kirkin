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

        internal int Capacity
        {
            get
            {
                return _capacity;
            }
            set
            {
                foreach (DataColumnLite column in Table.Columns) {
                    column.Data.Capacity = value;
                }

                _capacity = value;
            }
        }

        /// <summary>
        /// Creates a new row filled with nulls and adds it to the table.
        /// </summary>
        public DataRowLite AddNewRow()
        {
            EnsureSufficientCapacityForAdd();

            DataRowLite row = Table.CreateNewRow();

            row._rowIndex = Count;

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

            return Insert(_rows.Count, itemArray);
        }

        /// <summary>
        /// Creates a new row from the given item array and inserts it into the table at the given index.
        /// </summary>
        public DataRowLite Insert(int index, params object[] itemArray)
        {
            if (itemArray == null) throw new ArgumentNullException(nameof(itemArray));
            if (itemArray.Length != Table.Columns.Count) throw new ArgumentException("Item array length/column number mismatch.");

            EnsureSufficientCapacityForAdd();

            DataRowLite row = Table.CreateNewRow();

            row._rowIndex = index;

            _rows.Insert(index, row);

            // Fix up the indexes of all following rows.
            for (int i = index + 1; i < _rows.Count; i++) {
                _rows[i]._rowIndex = i;
            }

            // Move the data one level down.
            for (int i = _rows.Count - 1; i > index; i--)
            for (int j = 0; j < Table.Columns.Count; j++) {
                _rows[i][j] = _rows[i - 1][j];
            }

            // Fill the new row.
            for (int i = 0; i < itemArray.Length; i++) {
                row[i] = itemArray[i];
            }

            return row;
        }

        private void EnsureSufficientCapacityForAdd()
        {
            if (_rows.Count == _capacity)
            {
                int newCapacity = (_capacity == 0) ? DEFAULT_CAPACITY : _capacity * 2;

                Capacity = newCapacity;
            }
        }

        public void Remove(DataRowLite row)
        {
            int index = _rows.IndexOf(row);

            if (index == -1) {
                throw new ArgumentException("The row does not belong to this collection.");
            }

            foreach (DataColumnLite column in Table.Columns) {
                column.Data.Remove(index);
            }

            _rows.RemoveAt(index);

            // Fix up the indexes of the following rows.
            for (int i = index; i < _rows.Count; i++) {
                _rows[i]._rowIndex = i;
            }
        }

        public void Clear()
        {
            _rows.Clear();

            Capacity = DEFAULT_CAPACITY;
        }

        public void TrimExcess()
        {
            _rows.TrimExcess();

            Capacity = Count;
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