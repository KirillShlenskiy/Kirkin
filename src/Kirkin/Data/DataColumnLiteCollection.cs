using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Kirkin.Collections.Generic.Enumerators;
using Kirkin.Data.Internal;

namespace Kirkin.Data
{
    /// <summary>
    /// <see cref="DataColumnLite"/> collection.
    /// </summary>
    public sealed class DataColumnLiteCollection : Collection<DataColumnLite>
    {
        /// <summary>
        /// Gets the table that this column collection belongs to.
        /// </summary>
        internal readonly DataTableLite Table;

        private Dictionary<string, DataColumnLite> _columnMappingsFast;
        private Dictionary<string, DataColumnLite> _columnMappingsSlow;

        /// <summary>
        /// Resolves the column with the specified name.
        /// </summary>
        public DataColumnLite this[string name]
        {
            get
            {
                // This is the trick System.Data.DataColumnCollection uses.
                // Case-sensitive lookup is much faster than non-case-sensitive
                // lookup, so we'll run it first, and if we don't succeed - use
                // the more costly non-case-sensitive lookup as fallback.
                return _columnMappingsFast.TryGetValue(name, out DataColumnLite column)
                    ? column
                    : _columnMappingsSlow[name];
            }
        }

        internal DataColumnLiteCollection(DataTableLite table)
        {
            Table = table;

            RefreshColumnOrdinalMappings();
        }

        /// <summary>
        /// Creates a new column with the given name and data type, and adds it to the table.
        /// </summary>
        public DataColumnLite Add(string name, Type dataType)
        {
            DataColumnLite column = new DataColumnLite(name, dataType);

            Add(column);

            return column;
        }

        /// <summary>
        /// Returns true if a column with the given name is present in the collection.
        /// </summary>
        public bool Contains(string name)
        {
            return _columnMappingsFast.ContainsKey(name)
                || _columnMappingsSlow.ContainsKey(name);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();

            RefreshColumnOrdinalMappings();
        }

        /// <summary>
        /// Inserts an element into the collection at the specified index.
        /// </summary>
        protected override void InsertItem(int index, DataColumnLite item)
        {
            item.SetOwner(Table);

            base.InsertItem(index, item);

            RefreshColumnOrdinalMappings();
        }

        /// <summary>
        /// Removes the element at the specified index from the collection.
        /// </summary>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            RefreshColumnOrdinalMappings();
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        protected override void SetItem(int index, DataColumnLite item)
        {
            item.SetOwner(Table);

            base.SetItem(index, item);

            RefreshColumnOrdinalMappings();
        }

        private void RefreshColumnOrdinalMappings()
        {
            Dictionary<string, DataColumnLite> columnMappingsFast = new Dictionary<string, DataColumnLite>(Count);
            Dictionary<string, DataColumnLite> columnMappingsSlow = new Dictionary<string, DataColumnLite>(Count, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < Count; i++)
            {
                DataColumnLite column = this[i];

                columnMappingsFast.Add(column.ColumnName, column);
                columnMappingsSlow.Add(column.ColumnName, column);
            }

            _columnMappingsFast = columnMappingsFast;
            _columnMappingsSlow = columnMappingsSlow;
        }

        internal IColumnData GetColumnData(string columnName)
        {
            return this[columnName].Data;
        }

        /// <summary>
        /// Returns the struct enumerator over the underlying collection.
        /// </summary>
        public new ListEnumerator<DataColumnLite> GetEnumerator()
        {
            return new ListEnumerator<DataColumnLite>((List<DataColumnLite>)Items);
        }
    }
}