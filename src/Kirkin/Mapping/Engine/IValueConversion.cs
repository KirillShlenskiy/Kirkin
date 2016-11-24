using System;
using System.Linq.Expressions;

namespace Kirkin.Mapping.Engine
{
    /// <summary>
    /// Converts the return type of the expression to the given type.
    /// </summary>
    public interface IValueConversion
    {
        /// <summary>
        /// Converts the return type of the expression to the given type.
        /// </summary>
        bool TryConvert(Expression value, Type targetType, NullableBehaviour nullableBehaviour, out Expression result);
    }
}