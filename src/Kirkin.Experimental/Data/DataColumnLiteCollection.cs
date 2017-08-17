using System;
using System.Collections.Generic;

namespace Kirkin.Data
{
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
}