#if !__MOBILE__

using System;
using System.Data;
using System.Linq.Expressions;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping.Data
{
    /// <summary>
    /// TDataSource-based <see cref="Member"/> implementation.
    /// </summary>
    public abstract class DataMember : Member
    {
        #region Factory methods

        /// <summary>
        /// Resolves the member list from an <see cref="IDataRecord"/>.
        /// </summary>
        public static Member[] DataRecordMembers(IDataRecord dataRecord)
        {
            if (dataRecord == null) throw new ArgumentNullException(nameof(dataRecord));

            Member[] members = new Member[dataRecord.FieldCount];

            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                Type fieldType = TypeForMapping(dataRecord.GetFieldType(i), isNullable: true);

                members[i] = new GenericDataMember<IDataRecord>(dataRecord.GetName(i), fieldType);
            }

            return members;
        }

        /// <summary>
        /// Resolves the member list from an <see cref="DataTable"/>.
        /// </summary>
        public static Member[] DataTableMembers(DataTable dataTable)
        {
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));

            Member[] members = new Member[dataTable.Columns.Count];

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                DataColumn column = dataTable.Columns[i];
                Type columnType = TypeForMapping(column.DataType, column.AllowDBNull);

                members[i] = new GenericDataMember<DataRow>(column.ColumnName, columnType);
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

        #endregion

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
        private DataMember(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        sealed class GenericDataMember<TDataSource> : DataMember
        {
            internal GenericDataMember(string name, Type type)
                : base(name, type)
            {
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
                            typeof(TDataSource).GetProperty("Item", typeof(object), new[] { typeof(string) }),
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
}

#endif