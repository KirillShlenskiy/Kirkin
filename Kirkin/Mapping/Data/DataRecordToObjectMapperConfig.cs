#if !__MOBILE__

using System;
using System.Data;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping.Data
{
    /// <summary>
    /// <see cref="MapperConfig{TSource, TTarget}"/> implementation where
    /// the source is a <see cref="IDataRecord"/> and target is an object.
    /// </summary>
    public class DataRecordToObjectMapperConfig<TTarget> : MapperConfig<IDataRecord, TTarget>
    {
        /// <summary>
        /// <see cref="IDataRecord"/> used to create this instance.
        /// </summary>
        public IDataRecord DataRecord { get; }

        /// <summary>
        /// Creates a new instance of <see cref="DataRecordToObjectMapperConfig{TTarget}"/>.
        /// </summary>
        public DataRecordToObjectMapperConfig(IDataRecord dataRecord)
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