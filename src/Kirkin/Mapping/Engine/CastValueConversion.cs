using System;
using System.Linq.Expressions;

namespace Kirkin.Mapping.Engine
{
    internal sealed class CastValueConversion : IValueConversion
    {
        public bool TryConvert(Expression value, Type targetType, NullableBehaviour nullableBehaviour, out Expression result)
        {
            result = Expression.Convert(value, targetType);

            return true;
        }
    }
}