#if !__MOBILE__

using System.Data;

using Kirkin.Mapping.Engine;
using Kirkin.Mapping.Fluent;

namespace Kirkin.Mapping.Data
{
    /// <summary>
    /// Data-related fluent <see cref="MapperBuilderFactory"/> extension methods.
    /// </summary>
    internal static class DataMapperBuilderFactoryExtensions
    {
        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the given <see cref="IDataRecord"/> source to various target types. 
        /// </summary>
        public static PartiallyConfiguredMapperBuilder<IDataRecord> FromDataReaderOrRecord(this MapperBuilderFactory factory, IDataRecord dataRecord)
        {
            Member<IDataRecord>[] sourceMembers = DataMember.DataReaderOrRecordMembers(dataRecord);

            return new PartiallyConfiguredMapperBuilder<IDataRecord>(sourceMembers);
        }

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the given <see cref="DataTable"/> source to various target types. 
        /// </summary>
        public static PartiallyConfiguredMapperBuilder<DataRow> FromDataTable(this MapperBuilderFactory factory, DataTable dataTable)
        {
            Member<DataRow>[] sourceMembers = DataMember.DataTableMembers(dataTable);

            return new PartiallyConfiguredMapperBuilder<DataRow>(sourceMembers);
        }
    }
}

#endif