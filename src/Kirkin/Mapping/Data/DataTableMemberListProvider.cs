using System;
using System.Data;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping.Data
{
    internal sealed class DataTableMemberListProvider : IMemberListProvider<DataRow>
    {
        public DataTable DataTable { get; }

        public DataTableMemberListProvider(DataTable dataTable)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));

            DataTable = dataTable;
        }

        public Member[] GetMembers()
        {
            return DataMember.DataTableMembers(DataTable);
        }
    }
}