#if !__MOBILE__

using System;
using System.Data;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping.Data
{
    /// <summary>
    /// <see cref="MapperBuilder{TSource, TTarget}"/> implementation where
    /// the source is a <see cref="DataRow"/> and target is an object.
    /// </summary>
    public class DataRowToObjectMapperBuilder<TTarget> : MapperBuilder<DataRow, TTarget>
    {
        /// <summary>
        /// <see cref="DataTable"/> used to create this instance.
        /// </summary>
        public DataTable DataTable { get; }

        /// <summary>
        /// Creates a new instance of <see cref="DataRowToObjectMapperBuilder{TTarget}"/>.
        /// </summary>
        public DataRowToObjectMapperBuilder(DataTable dataTable)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));

            DataTable = dataTable;

            // Overrides.
            MappingMode = MappingMode.AllTargetMembers;
            MemberNameComparer = StringComparer.OrdinalIgnoreCase;
        }

        /// <summary>
        /// Gets the default source member list for this instance.
        /// </summary>
        protected override Member[] GetSourceMembers()
        {
            return DataMember.DataTableMembers(DataTable);
        }
    }
}

#endif