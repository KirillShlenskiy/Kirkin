using System;
using System.Linq.Expressions;

using Kirkin.Reflection;
using Kirkin.Mapping.Engine;

#if !__MOBILE__
using System.Data;
using Kirkin.Mapping.Data;
#endif

namespace Kirkin.Mapping
{
    /// <summary>
    /// <see cref="MapperBuilder{TSource, TTarget}"/> factory methods.
    /// </summary>
    public static class MapperBuilder
    {
#if !__MOBILE__
        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the given <see cref="IDataRecord"/> source to various target types. 
        /// </summary>
        public static MapperBuilderFactory<IDataRecord> FromDataReaderOrRecord(IDataRecord dataRecord)
        {
            Member[] sourceMembers = DataMember.DataRecordMembers(dataRecord);

            return new DataMapperBuilderFactory<IDataRecord>(sourceMembers);
        }

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the given <see cref="DataTable"/> source to various target types. 
        /// </summary>
        public static MapperBuilderFactory<DataRow> FromDataTable(DataTable dataTable)
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
#endif
        /// <summary>
        /// Expression-based <see cref="MapperBuilder{TSource, TTarget}"/> factory placeholder.
        /// </summary>
        internal static MapperBuilder<TSource, TTarget> FromExpression<TSource, TTarget>(Expression<Func<TSource, TTarget>> expression)
        {
            throw new NotImplementedException(); // TODO.
        }

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from object sources of the given type to various target types. 
        /// </summary>
        public static MapperBuilderFactory<TSource> FromType<TSource>()
        {
            Member[] sourceMembers = PropertyMember.PublicInstanceProperties<TSource>();

            return new MapperBuilderFactory<TSource>(sourceMembers);
        }

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the specified properties of the given type to various target types.
        /// </summary>
        public static MapperBuilderFactory<TSource> FromPropertyList<TSource>(PropertyList<TSource> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            Member[] sourceMembers = PropertyMember.MembersFromPropertyList(propertyList);

            return new MapperBuilderFactory<TSource>(sourceMembers);
        }
    }
}