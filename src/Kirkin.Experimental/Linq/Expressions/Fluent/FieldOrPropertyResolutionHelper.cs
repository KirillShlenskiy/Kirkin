using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.Linq.Expressions.Fluent
{
    public sealed class FieldOrPropertyResolutionHelper<T>
    {
        internal static readonly FieldOrPropertyResolutionHelper<T> Instance = new FieldOrPropertyResolutionHelper<T>();

        private FieldOrPropertyResolutionHelper()
        {
        }

        public Expression<Func<T, TMember>> Getter<TMember>(MemberInfo member)
        {
            return ExpressionEngine.Getter<T, TMember>(member);
        }

        public Expression<Func<T, TMember>> Getter<TMember>(string name, bool nonPublic = false, bool ignoreCase = false)
        {
            BindingFlags bindingFlags = nonPublic
                ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                : BindingFlags.Instance | BindingFlags.Public;

            if (ignoreCase) {
                bindingFlags |= BindingFlags.IgnoreCase;
            }

            MemberInfo member = (MemberInfo)typeof(T).GetProperty(name, bindingFlags) ?? typeof(T).GetField(name, bindingFlags);

            if (member == null) {
                throw new InvalidOperationException("Unable to resolve given member.");
            }

            return Getter<TMember>(member);
        }

        public Expression<Func<T, TMember>> Getter<TMember>(Expression<Func<T, TMember>> expression)
        {
            // Here for completeness.
            return expression;
        }

        public Expression<Action<T, TMember>> Setter<TMember>(MemberInfo member)
        {
            return ExpressionEngine.Setter<T, TMember>(member);
        }

        public Expression<Action<T, TMember>> Setter<TMember>(string name, bool nonPublic = false, bool ignoreCase = false)
        {
            BindingFlags bindingFlags = nonPublic
                ? BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                : BindingFlags.Instance | BindingFlags.Public;

            if (ignoreCase) {
                bindingFlags |= BindingFlags.IgnoreCase;
            }

            MemberInfo member = (MemberInfo)typeof(T).GetProperty(name, bindingFlags) ?? typeof(T).GetField(name, bindingFlags);

            if (member == null) {
                throw new InvalidOperationException("Unable to resolve given member.");
            }

            return Setter<TMember>(member);
        }

        public Expression<Action<T, TMember>> Setter<TMember>(Expression<Func<T, TMember>> expression)
        {
            return ExpressionEngine.Setter<T, TMember>(ExpressionUtil.Member(expression));
        }
    }
}