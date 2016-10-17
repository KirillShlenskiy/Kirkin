using System;
using System.Linq.Expressions;

namespace Kirkin.Refs
{
    public static class Ref
    {
        public static IRef<T> FromExpression<T>(Expression<Func<T>> expr)
        {
            Func<T> getter = expr.Compile();
            ParameterExpression valueParam = Expression.Parameter(typeof(T), "value");

            Action<T> setter = Expression
                .Lambda<Action<T>>(Expression.Assign(expr.Body, valueParam), valueParam)
                .Compile();

            return new Ref<T>(getter, setter);
        }
    }

    sealed class Ref<T> : IRef<T>
    {
        private readonly Func<T> Getter;
        private readonly Action<T> Setter;

        public T Value
        {
            get
            {
                return Getter();
            }
            set
            {
                Setter(value);
            }
        }

        object IRef.Value
        {
            get
            {
                return Value;
            }
            set
            {
                Value = (T)value;
            }
        }

        internal Ref(Func<T> getter, Action<T> setter)
        {
            Getter = getter;
            Setter = setter;
        }
    }
}