using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Kirkin.Collections.Generic.Enumerators;

namespace Kirkin.Data
{
    /// <summary>
    /// Row collection.
    /// </summary>
    public sealed class DataRowLiteCollection : Collection<DataRowLite>
    {
        private const int DEFAULT_CAPACITY = 16;

        private readonly DataTableLite Table;
        private int _capacity;

        internal DataRowLiteCollection(DataTableLite table)
            : base(new List<DataRowLite>(DEFAULT_CAPACITY))
        {
            Table = table;
        }

        /// <summary>
        /// Gets or sets the capacity of the data storage arrays.
        /// </summary>
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

            base.Add(row);

            return row;
        }

        /// <summary>
        /// Creates a new row from the given item array and adds it to the table.
        /// </summary>
        public DataRowLite Add(params object[] itemArray)
        {
            if (itemArray == null) throw new ArgumentNullException(nameof(itemArray));
            if (itemArray.Length != Table.Columns.Count) throw new ArgumentException("Item array length/column number mismatch.");

            return Insert(Count, itemArray);
        }

        /// <summary>
        /// Inserts a row into the collection at the specified index.
        /// </summary>
        protected override void InsertItem(int index, DataRowLite item)
        {
            if (item.Table != Table) {
                throw new ArgumentException("The given row does not belong to this table.");
            }

            base.InsertItem(index, item);

            item._rowIndex = index;

            // Fix up the indexes of all following rows.
            for (int i = index + 1; i < Count; i++) {
                this[i]._rowIndex = i;
            }

            // Move the data one level down.
            for (int i = Count - 1; i > index; i--)
            for (int j = 0; j < Table.Columns.Count; j++) {
                this[i][j] = this[i - 1][j];
            }
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

            InsertItem(index, row);

            // Fill the new row.
            for (int i = 0; i < itemArray.Length; i++) {
                row[i] = itemArray[i];
            }

            return row;
        }

        /// <summary>
        /// Removes the row at the specified index of the collection.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            if (index == -1) {
                throw new ArgumentException("The row does not belong to this collection.");
            }

            foreach (DataColumnLite column in Table.Columns) {
                column.Data.Remove(index);
            }

            base.RemoveItem(index);

            // Fix up the indexes of the following rows.
            for (int i = index; i < Count; i++) {
                this[i]._rowIndex = i;
            }
        }

        /// <summary>
        /// Removes all rows from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (DataRowLite row in this) {
                row._rowIndex = -1;
            }

            base.ClearItems();

            Capacity = DEFAULT_CAPACITY;
        }

        /// <summary>
        /// Suppressed.
        /// </summary>
        protected override void SetItem(int index, DataRowLite item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the collection.
        /// </summary>
        public void TrimExcess()
        {
            List<DataRowLite> items = (List<DataRowLite>)Items;

            items.TrimExcess();

            Capacity = Count;
        }

        /// <summary>
        /// Returns a fast struct enumerator over this collection.
        /// </summary>
        /// <returns></returns>
        public ListEnumerator<DataRowLite> GetEnumerator()
        {
            return new ListEnumerator<DataRowLite>((List<DataRowLite>)Items);
        }

        /// <summary>
        /// Doubles the capacity of the data arrays if needed.
        /// </summary>
        private void EnsureSufficientCapacityForAdd()
        {
            if (Count == _capacity)
            {
                int newCapacity = (_capacity == 0) ? DEFAULT_CAPACITY : _capacity * 2;

                Capacity = newCapacity;
            }
        }
    }
}