#if !__MOBILE__

using System;
using System.Data;

using Kirkin.Mapping.Engine;
using Kirkin.Mapping.Fluent;

namespace Kirkin.Mapping.Data
{
    /// <summary>
    /// Data-related fluent <see cref="MapperBuilderFactory"/> extension methods.
    /// </summary>
    public static class DataMapperBuilderFactoryExtensions
    {
        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the given <see cref="IDataRecord"/> source to various target types. 
        /// </summary>
        public static MapperBuilderFactory<IDataRecord> FromDataReaderOrRecord(this MapperBuilderFactory factory, IDataRecord dataRecord)
        {
            Member[] sourceMembers = DataMember.DataRecordMembers(dataRecord);

            return new DataMapperBuilderFactory<IDataRecord>(sourceMembers);
        }

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the given <see cref="DataTable"/> source to various target types. 
        /// </summary>
        public static MapperBuilderFactory<DataRow> FromDataTable(this MapperBuilderFactory factory, DataTable dataTable)
        {
            Member[] sourceMembers = DataMember.DataTableMembers(dataTable);

            return new DataMapperBuilderFactory<DataRow>(sourceMembers);
        }

        sealed class DataMapperBuilderFactory<TSource> : MapperBuilderFactory<TSource>
        {
            internal DataMapperBuilderFactory(Member[] sourceMembers)
                : base(sourceMembers)
            {
            }

            protected override MapperBuilder<TSource, TTarget> CreateAndConfigureBuilder<TTarget>(Member[] targetMembers)
            {
                MapperBuilder<TSource, TTarget> builder = base.CreateAndConfigureBuilder<TTarget>(targetMembers);

                builder.MapAllSourceMembers = false;
                builder.MapAllTargetMembers = true;
                builder.MemberNameComparer = StringComparer.OrdinalIgnoreCase;

                return builder;
            }
        }
    }
}

#endif