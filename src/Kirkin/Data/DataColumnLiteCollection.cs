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

        private Dictionary<string, int> _columnNameToIndexMappings;
        private Dictionary<string, IColumnData> _columnNameToDataMappings;

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
            Dictionary<string, int> dict1 = new Dictionary<string, int>(Count, StringComparer.OrdinalIgnoreCase);
            Dictionary<string, IColumnData> dict2 = new Dictionary<string, IColumnData>(Count, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < Count; i++)
            {
                dict1.Add(this[i].ColumnName, i);
                dict2.Add(this[i].ColumnName, this[i].Data);
            }

            _columnNameToIndexMappings = dict1;
            _columnNameToDataMappings = dict2;
        }

        internal IColumnData GetColumnData(string columnName)
        {
            //return this[_columnNameToIndexMappings[columnName]].Data;
            return _columnNameToDataMappings[columnName];
        }
    }
}