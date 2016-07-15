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
        /// Creates a new <see cref="MapperBuilder{TSource, TTarget}"/> where
        /// the source is a <see cref="IDataRecord"/> and target is an object.
        /// </summary>
        internal static MapperBuilderFactory<IDataRecord> FromDataReaderOrRecord(IDataRecord dataRecord)
        {
            Member[] sourceMembers = DataMember.DataRecordMembers(dataRecord);

            return new DataMapperBuilderFactory<IDataRecord>(sourceMembers);
        }

        /// <summary>
        /// Creates a new <see cref="MapperBuilder{TSource, TTarget}"/> where
        /// the source is a <see cref="DataRow"/> and target is an object.
        /// </summary>
        internal static MapperBuilderFactory<DataRow> FromDataTable(DataTable dataTable)
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

                builder.MappingMode = MappingMode.AllTargetMembers;
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
        /// Creates a new <see cref="MapperBuilder{TSource, TTarget}"/> instance with the
        /// same source and target type, mapping all the properties in the given list.
        /// </summary>
        public static MapperBuilder<T, T> FromPropertyList<T>(PropertyList<T> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            Member[] members = new Member[propertyList.Properties.Length];

            for (int i = 0; i < propertyList.Properties.Length; i++) {
                members[i] = new PropertyMember(propertyList.Properties[i]);
            }

            return new MapperBuilder<T, T>(members, members) {
                MappingMode = MappingMode.Relaxed // No need to validate mapping.
            };
        }
    }
}