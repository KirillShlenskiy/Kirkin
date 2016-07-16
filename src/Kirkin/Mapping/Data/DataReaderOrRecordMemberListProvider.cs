using System;
using System.Data;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping.Data
{
    internal sealed class DataReaderOrRecordMemberListProvider : IMemberListProvider<IDataRecord>
    {
        public IDataRecord DataRecord { get; }

        public DataReaderOrRecordMemberListProvider(IDataRecord dataRecord)
        {
            if (dataRecord == null) throw new ArgumentNullException(nameof(dataRecord));

            DataRecord = dataRecord;
        }

        public Member<IDataRecord>[] GetMembers()
        {
            return DataMember.DataRecordMembers(DataRecord);
        }
    }
}