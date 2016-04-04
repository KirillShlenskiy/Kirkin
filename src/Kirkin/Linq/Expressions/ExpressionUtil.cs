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
        [Obsolete("Use Member instead.")]
        public static string MemberName<T, TMember>(Expression<Func<T, TMember>> expr)
        {
            return Member(expr).Name;
        }

        /// <summary>
        /// Resolves the name of the member specified by the given member expression.
        /// </summary>
        [Obsolete("Use Member instead.")]
        public static string MemberName<TMember>(Expression<Func<TMember>> expr)
        {
            return Member(expr).Name;
        }

        /// <summary>
        /// Resolves the name of the member specified by the given member expression.
        /// </summary>
        [Obsolete("Use Member instead.")]
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
        [Obsolete("Use Member instead.")]
        public static string MemberName(Expression<Func<object>> expr)
        {
            return Member(expr).Name;
        }

        #endregion

        #region Method

        /// <summary>
        /// Resolves the <see cref="MethodInfo"/> from the given method call expression.
        /// </summary>
        public static MethodInfo Method<T>(Expression<Action<T>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            return MethodFromExpression(expr.Body);
        }

        /// <summary>
        /// Resolves the <see cref="MethodInfo"/> from the given method call expression.
        /// </summary>
        public static MethodInfo Method<T>(Expression<Func<T, object>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            UnaryExpression convertExpr = expr.Body as UnaryExpression;

            return convertExpr == null
                ? MethodFromExpression(expr.Body)
                // Assuming struct return type. Method expression will be
                // wrapped by Expression.Convert(<methodExpr>, typeof(object)).
                : MethodFromExpression(convertExpr.Operand);
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

        #endregion
    }
}