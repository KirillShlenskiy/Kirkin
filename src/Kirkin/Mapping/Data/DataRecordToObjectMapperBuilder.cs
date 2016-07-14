#if !__MOBILE__

using System;
using System.Data;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping.Data
{
    /// <summary>
    /// <see cref="MapperBuilder{TSource, TTarget}"/> implementation where
    /// the source is a <see cref="IDataRecord"/> and target is an object.
    /// </summary>
    public class DataRecordToObjectMapperBuilder<TTarget> : MapperBuilder<IDataRecord, TTarget>
    {
        /// <summary>
        /// <see cref="IDataRecord"/> used to create this instance.
        /// </summary>
        public IDataRecord DataRecord { get; }

        /// <summary>
        /// Creates a new instance of <see cref="DataRecordToObjectMapperBuilder{TTarget}"/>.
        /// </summary>
        public DataRecordToObjectMapperBuilder(IDataRecord dataRecord)
        {
            if (dataRecord == null) throw new ArgumentNullException(nameof(dataRecord));

            DataRecord = dataRecord;

            // Overrides.
            MappingMode = MappingMode.AllTargetMembers;
            MemberNameComparer = StringComparer.OrdinalIgnoreCase;
        }

        /// <summary>
        /// Gets the default source member list for this instance.
        /// </summary>
        protected override Member[] GetSourceMembers()
        {
            return DataMember.DataRecordMembers(DataRecord);
        }
    }
}

#endif