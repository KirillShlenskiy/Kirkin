using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kirkin.Data
{
    public sealed class DataColumnLiteCollection : Collection<DataColumnLite>
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
                return this[ColumnNameToIndexMappings[name]];
            }
        }

        internal DataColumnLiteCollection(DataTableLite table)
        {
            Table = table;
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

            __columnNameToIndexMappings = null;
        }

        protected override void InsertItem(int index, DataColumnLite item)
        {
            base.InsertItem(index, item);

            __columnNameToIndexMappings = null;
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            __columnNameToIndexMappings = null;
        }

        protected override void SetItem(int index, DataColumnLite item)
        {
            base.SetItem(index, item);

            __columnNameToIndexMappings = null;
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
}