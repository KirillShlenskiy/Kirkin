#if !__MOBILE__

using System;
using System.Data;
using System.Linq.Expressions;

namespace Kirkin.Mapping.Data
{
    /// <summary>
    /// Data-related <see cref="Member{T}"/> factory methods. 
    /// </summary>
    public static class DataMember
    {
        /// <summary>
        /// Resolves the member list from an <see cref="IDataRecord"/>.
        /// </summary>
        public static Member<IDataRecord>[] DataReaderOrRecordMembers(IDataRecord dataRecord)
        {
            if (dataRecord == null) throw new ArgumentNullException(nameof(dataRecord));

            Member<IDataRecord>[] members = new Member<IDataRecord>[dataRecord.FieldCount];

            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                Type fieldType = TypeForMapping(dataRecord.GetFieldType(i), isNullable: true);

                members[i] = new DataMember<IDataRecord>(dataRecord.GetName(i), fieldType);
            }

            return members;
        }

        /// <summary>
        /// Resolves the member list from an <see cref="DataTable"/>.
        /// </summary>
        public static Member<DataRow>[] DataTableMembers(DataTable dataTable)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));

            Member<DataRow>[] members = new Member<DataRow>[dataTable.Columns.Count];

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                DataColumn column = dataTable.Columns[i];
                Type columnType = TypeForMapping(column.DataType, column.AllowDBNull);

                members[i] = new DataMember<DataRow>(column.ColumnName, columnType);
            }

            return members;
        }

        /// <summary>
        /// Resolves the appropriate CLR type to ensure that
        /// nullable value type data columns are treated as such.
        /// </summary>
        private static Type TypeForMapping(Type concreteType, bool isNullable)
        {
            if (isNullable && concreteType.IsValueType) {
                return typeof(Nullable<>).MakeGenericType(concreteType);
            }

            return concreteType;
        }
    }

    /// <summary>
    /// TDataSource-based <see cref="Member{T}"/> implementation.
    /// </summary>
    internal sealed class DataMember<T> : Member<T>
    {
        /// <summary>
        /// Returns true if this member supports read operations.
        /// </summary>
        public override bool CanRead { get; } = true;

        /// <summary>
        /// Returns true if this member supports write operations.
        /// </summary>
        public override bool CanWrite { get; } = false;

        /// <summary>
        /// Name of the mapped member.
        /// </summary>
        public override string Name { get; }

        /// <summary>
        /// Runtime type of the member.
        /// </summary>
        public override Type Type { get; }

        /// <summary>
        /// Creates a new instance of <see cref="DataMember"/>.
        /// </summary>
        internal DataMember(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Produces an expression which retrieves the relevant member value from the source.
        /// </summary>
        protected internal override Expression ResolveGetter(ParameterExpression source)
        {
            // Goal tree:

            // record =>
            // {
            //     int result;
            //     object value = record["FieldName"];

            //     if (value != DBNull.Value) {
            //         result = (int)value;
            //     }

            //     return result;
            // };

            ParameterExpression result = Expression.Parameter(Type, nameof(result));
            ParameterExpression value = Expression.Parameter(typeof(object), nameof(value));

            return Expression.Block(
                new[] { result, value },
                Expression.Assign(
                    value,
                    Expression.MakeIndex(
                        source,
                        typeof(T).GetProperty("Item", typeof(object), new[] { typeof(string) }),
                        new[] { Expression.Constant(Name) }
                    )
                ),
                Expression.IfThen(
                    Expression.NotEqual(value, Expression.Constant(DBNull.Value)),
                    Expression.Assign(result, Expression.Convert(value, Type))
                ),
                result
            );
        }

        /// <summary>
        /// Produces an expression which stores the value in the relevant member of the target.
        /// </summary>
        protected internal override Expression ResolveSetter(ParameterExpression target)
        {
            throw new NotSupportedException();
        }
    }
}

#endif