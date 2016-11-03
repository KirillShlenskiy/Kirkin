using System;
using System.Linq.Expressions;

using Kirkin.Reflection;

namespace Kirkin.Mapping
{
    /// <summary>
    /// <see cref="Member{T}"/> factory methods.
    /// </summary>
    public static class Member
    {
        #region Single member factory methods

        public static Member<TObject> ReadOnly<TObject, TValue>(string name, Expression<Func<TObject, TValue>> getter)
        {
            return new ExpressionMember<TObject, TValue>(name, getter);
        }

        internal static Member<TObject> ReadOnlyDelegate<TObject, TValue>(string name, Func<TObject, TValue> getter)
        {
            return DelegateMember.ReadOnly(name, getter);
        }

        public static Member<TObject> ReadWrite<TObject, TValue>(string name, Expression<Func<TObject, TValue>> getter, Action<TObject, TValue> setter)
        {
            // TODO: Proper hybrid expression getter / action setter implementation.
            return ReadWriteDelegate(name, getter.Compile(), setter);
        }

        internal static Member<TObject> ReadWriteDelegate<TObject, TValue>(string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
        {
            return DelegateMember.ReadWrite(name, getter, setter);
        }

        public static Member<TObject> WriteOnly<TObject, TValue>(string name, Action<TObject, TValue> setter)
        {
            return DelegateMember.WriteOnly(name, setter);
        }

        #endregion

        #region Member list factory methods

        public static Member<T>[] FromPublicInstanceProperties<T>()
        {
            return PropertyMember.PublicInstanceProperties<T>();
        }

        public static Member<T>[] FromPropertyList<T>(PropertyList<T> propertyList)
        {
            return PropertyMember.PropertyListMembers(propertyList);
        }

        #endregion
    }
}