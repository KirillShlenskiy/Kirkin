using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Kirkin.Data.Internal;

namespace Kirkin.Data
{
    public sealed class DataColumnLiteCollection : Collection<DataColumnLite>
    {
        /// <summary>
        /// Gets the table that this column collection belongs to.
        /// </summary>
        internal readonly DataTableLite Table;

        private Dictionary<string, DataColumnLite> _columnMappings;

        /// <summary>
        /// Resolves the column with the specified name.
        /// </summary>
        public DataColumnLite this[string name]
        {
            get
            {
                return _columnMappings[name];
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
            return _columnMappings.ContainsKey(name);
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
            Dictionary<string, DataColumnLite> dict = new Dictionary<string, DataColumnLite>(Count, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < Count; i++)
            {
                DataColumnLite column = this[i];

                dict.Add(column.ColumnName, column);
            }

            _columnMappings = dict;
        }

        internal IColumnData GetColumnData(string columnName)
        {
            //return this[_columnNameToIndexMappings[columnName]].Data;
            return _columnMappings[columnName].Data;
        }
    }
}