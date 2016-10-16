using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;

using Kirkin.Mapping;

namespace Kirkin.Data.SqlClient
{
    internal static class SqlCommandParameterMapping
    {
        public static void FillCommandParameters<T>(SqlCommand command, T obj, Action<MapperBuilder<T, SqlCommand>> configAction = null)
        {
            Member<SqlCommand>[] parameterMembers = ResolveMembers(command);

            MapperBuilder<T, SqlCommand> builder = Mapper.Builder
                .From<T>()
                .To(parameterMembers);

            builder.AllowUnmappedTargetMembers = true;
            builder.MemberNameComparer = StringComparer.OrdinalIgnoreCase;

            configAction?.Invoke(builder);

            Mapper<T, SqlCommand> mapper = builder.BuildMapper();

            mapper.Map(obj, command);
        }

        public static Member<SqlCommand>[] ResolveMembers(SqlCommand command)
        {
            if (command.Connection == null) throw new ArgumentException("Command connection must be non-null.");

            if (command.Connection.State != ConnectionState.Open) {
                command.Connection.Open();
            }

            SqlCommandBuilder.DeriveParameters(command);
            List<Member<SqlCommand>> members = new List<Member<SqlCommand>>(command.Parameters.Count);

            foreach (SqlParameter parameter in command.Parameters) {
                members.Add(new SqlParameterMember(parameter));
            }

            return members.ToArray();
        }

        sealed class SqlParameterMember : Member<SqlCommand>
        {
            private readonly SqlParameter Parameter;

            public override bool CanRead => true;
            public override bool CanWrite => true;

            public override string Name
            {
                get
                {
                    return Parameter.ParameterName.TrimStart('@');
                }
            }

            public override Type MemberType
            {
                get
                {
                    return typeof(object);
                }
            }

            internal SqlParameterMember(SqlParameter parameter)
            {
                Parameter = parameter;
            }

            protected internal override Expression ResolveGetter(ParameterExpression source)
            {
                // Do we even need the getter?
                return ResolveSetter(source);
            }

            protected internal override Expression ResolveSetter(ParameterExpression target)
            {
                // Source is SqlCommand.
                Expression<Func<SqlCommand, object>> expr = command => command.Parameters["@" + Name].Value;
                Expression finalExpr = new SubstituteParameterVisitor(target).Visit(expr.Body);

                return finalExpr;
            }

            sealed class SubstituteParameterVisitor : ExpressionVisitor
            {
                public readonly ParameterExpression NewParameterExpression;

                internal SubstituteParameterVisitor(ParameterExpression newParameterExpression)
                {
                    NewParameterExpression = newParameterExpression;
                }

                protected override Expression VisitParameter(ParameterExpression node)
                {
                    return NewParameterExpression;
                }
            }
        }
    }
}