using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.Linq.Expressions
{
    /// <summary>
    /// Common utility methods for processing expression trees.
    /// </summary>
    public static class ExpressionUtil
    {
        #region Member

        /// <summary>
        /// Resolves the MemberInfo from the given member expression.
        /// </summary>
        public static MemberInfo Member<T, TMember>(Expression<Func<T, TMember>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            return MemberFromExpression(expr.Body);
        }

        /// <summary>
        /// Resolves the MemberInfo from the given member expression.
        /// </summary>
        public static MemberInfo Member<TMember>(Expression<Func<TMember>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            return MemberFromExpression(expr.Body);
        }

        /// <summary>
        /// Resolves the MemberInfo from the given member expression.
        /// </summary>
        public static MemberInfo Member<T>(Expression<Func<T, object>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            UnaryExpression convertExpr = expr.Body as UnaryExpression;

            return convertExpr == null
                ? MemberFromExpression(expr.Body)
                // Assuming struct return type. Member expression will be
                // wrapped by Expression.Convert(<memberExpr>, typeof(object)).
                : MemberFromExpression(convertExpr.Operand);
        }

        /// <summary>
        /// Resolves the MemberInfo from the given member expression.
        /// </summary>
        /// <remarks>
        /// Needed for when the Func return type is unknown.
        /// </remarks>
        public static MemberInfo Member(Expression<Func<object>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            UnaryExpression convertExpr = expr.Body as UnaryExpression;

            return convertExpr == null
                ? MemberFromExpression(expr.Body)
                // Assuming struct return type. Member expression will be
                // wrapped by Expression.Convert(<memberExpr>, typeof(object)).
                : MemberFromExpression(convertExpr.Operand);
        }

        /// <summary>
        /// Casts the given expression to <see cref="MemberExpression"/>
        /// and returns its <see cref="MemberExpression.Member" />.
        /// </summary>
        private static MemberInfo MemberFromExpression(Expression expr)
        {
            MemberExpression memberExpression = expr as MemberExpression;

            if (memberExpression == null) {
                throw new InvalidOperationException("The expression is not a MemberExpression.");
            }

            return memberExpression.Member;
        }

        #endregion

        #region MemberName

        /// <summary>
        /// Resolves the name of the member specified by the given member expression.
        /// </summary>
#if !ALLOW_MEMBER_NAME
        [Obsolete("Use Member instead.")]
#endif
        public static string MemberName<T, TMember>(Expression<Func<T, TMember>> expr)
        {
            return Member(expr).Name;
        }

        /// <summary>
        /// Resolves the name of the member specified by the given member expression.
        /// </summary>
#if !ALLOW_MEMBER_NAME
        [Obsolete("Use Member instead.")]
#endif
        public static string MemberName<TMember>(Expression<Func<TMember>> expr)
        {
            return Member(expr).Name;
        }

        /// <summary>
        /// Resolves the name of the member specified by the given member expression.
        /// </summary>
#if !ALLOW_MEMBER_NAME
        [Obsolete("Use Member instead.")]
#endif
        public static string MemberName<T>(Expression<Func<T, object>> expr)
        {
            return Member(expr).Name;
        }

        /// <summary>
        /// Resolves the name of the member specified by the given member expression.
        /// </summary>
        /// <remarks>
        /// Needed for when the Func return type is unknown.
        /// </remarks>
#if !ALLOW_MEMBER_NAME
        [Obsolete("Use Member instead.")]
#endif
        public static string MemberName(Expression<Func<object>> expr)
        {
            return Member(expr).Name;
        }

        #endregion

        #region Method

        /// <summary>
        /// Resolves the <see cref="MethodInfo"/> for the instance
        /// method extracted from the given method call expression.
        /// Works for both void and value-returning methods.
        /// </summary>
        public static MethodInfo InstanceMethod<T>(Expression<Action<T>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            return MethodFromExpression(expr.Body);
        }

        /// <summary>
        /// Resolves the <see cref="MethodInfo"/> for the static
        /// method extracted from the given method call expression.
        /// Works for both void and value-returning methods.
        /// </summary>
        public static MethodInfo StaticMethod(Expression<Action> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            return MethodFromExpression(expr.Body);
        }

        /// <summary>
        /// Casts the given expression to <see cref="MethodCallExpression"/>
        /// and returns its <see cref="MethodCallExpression.Method" />.
        /// </summary>
        private static MethodInfo MethodFromExpression(Expression expr)
        {
            MethodCallExpression methodCallExpression = expr as MethodCallExpression;

            if (methodCallExpression == null) {
                throw new InvalidOperationException("The expression is not a MethodCallExpression.");
            }

            return methodCallExpression.Method;
        }

        #endregion

        #region Property

        /// <summary>
        /// Resolves the PropertyInfo from the given property expression.
        /// </summary>
        public static PropertyInfo Property<T, TProperty>(Expression<Func<T, TProperty>> expr)
        {
            return PropertyFromMember(Member(expr));
        }

        /// <summary>
        /// Resolves the PropertyInfo from the given property expression.
        /// </summary>
        public static PropertyInfo Property<TProperty>(Expression<Func<TProperty>> expr)
        {
            return PropertyFromMember(Member(expr));
        }

        /// <summary>
        /// Resolves the PropertyInfo from the given property expression.
        /// </summary>
        public static PropertyInfo Property<T>(Expression<Func<T, object>> expr)
        {
            return PropertyFromMember(Member(expr));
        }

        /// <summary>
        /// Resolves the PropertyInfo from the given property expression.
        /// </summary>
        public static PropertyInfo Property(Expression<Func<object>> expr)
        {
            return PropertyFromMember(Member(expr));
        }

        private static PropertyInfo PropertyFromMember(MemberInfo member)
        {
            PropertyInfo prop = member as PropertyInfo;

            if (prop == null) {
                throw new InvalidOperationException("Specified member is not a property.");
            }

            return prop;
        }

        #endregion

        #region Experimental

        /// <summary>
        /// Produces a predicate expression whose result is the opposite
        /// of the result produced by the given predicate expression.
        /// </summary>
        public static Expression<Func<T, bool>> Not<T>(Expression<Func<T, bool>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            return Expression.Lambda<Func<T, bool>>(Expression.Not(expr.Body), expr.Parameters);
        }

        /// <summary>
        /// Rewrites the given expression replacing all field and member access
        /// sub-expressions with their actual values (as constant expressions).
        /// </summary>
        internal static Expression<TDelegate> ResolveAllFieldAndPropertyValuesAsConstants<TDelegate>(Expression<TDelegate> expr)
        {
            Expression newBody = ResolveAllFieldAndPropertyValuesAsConstantsVisitor.Instance.Visit(expr.Body);

            if (newBody == expr.Body) {
                return expr; // Unmodified.
            }

            return Expression.Lambda<TDelegate>(newBody);
        }

        sealed class ResolveAllFieldAndPropertyValuesAsConstantsVisitor : ExpressionVisitor
        {
            internal static readonly ResolveAllFieldAndPropertyValuesAsConstantsVisitor Instance
                = new ResolveAllFieldAndPropertyValuesAsConstantsVisitor();

            protected override Expression VisitMember(MemberExpression node)
            {
                ConstantExpression constExpr = node.Expression as ConstantExpression;

                if (node.Expression is MemberExpression memberExpr)
                {
                    Expression memberValueExpr = VisitMember(memberExpr); // Reduce.

                    if (memberValueExpr is ConstantExpression newConstExpr)
                    {
                        constExpr = newConstExpr;
                    }
                    else
                    {
                        return memberValueExpr;
                    }
                }

                if (constExpr != null || node.Expression == null) // node.Expiression = null means static member access.
                {
                    object obj = constExpr?.Value;

                    if (node.Member is PropertyInfo prop) {
                        return Expression.Constant(prop.GetValue(obj, null));
                    }

                    if (node.Member is FieldInfo field) {
                        return Expression.Constant(field.GetValue(obj));
                    }
                }

                return base.VisitMember(node);
            }
        }

        #endregion
    }
}