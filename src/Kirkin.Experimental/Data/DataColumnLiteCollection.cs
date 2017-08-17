using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kirkin.Data
{
    public sealed class DataColumnLiteCollection : Collection<DataColumnLite>
    {
        private Dictionary<string, int> _columnNameToIndexMappings;

        /// <summary>
        /// Gets the table that this column collection belongs to.
        /// </summary>
        public DataTableLite Table { get; }

        /// <summary>
        /// Resolves the column with the specified name.
        /// </summary>
        public DataColumnLite this[string name]
        {
            get
            {
                return this[_columnNameToIndexMappings[name]];
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

        protected override void ClearItems()
        {
            base.ClearItems();

            RefreshColumnOrdinalMappings();
        }

        protected override void InsertItem(int index, DataColumnLite item)
        {
            base.InsertItem(index, item);

            RefreshColumnOrdinalMappings();

            item.Data.Capacity = Table.Rows.Capacity;
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            RefreshColumnOrdinalMappings();
        }

        protected override void SetItem(int index, DataColumnLite item)
        {
            base.SetItem(index, item);

            RefreshColumnOrdinalMappings();

            item.Data.Capacity = Table.Rows.Capacity;
        }

        private void RefreshColumnOrdinalMappings()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>(Count, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < Count; i++) {
                dict.Add(this[i].ColumnName, i);
            }

            _columnNameToIndexMappings = dict;
        }
    }
}