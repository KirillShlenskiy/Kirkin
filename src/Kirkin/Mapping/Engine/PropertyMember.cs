using System;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Reflection;

namespace Kirkin.Mapping.Engine
{
    /// <summary>
    /// <see cref="PropertyInfo"/>-based <see cref="Member"/> implementation.
    /// </summary>
    public sealed class PropertyMember<T> : Member<T>
    {
        /// <summary>
        /// Resolves the default member list for type (public instance properties).
        /// </summary>
        public static PropertyMember<T>[] PublicInstanceProperties()
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            PropertyMember<T>[] members = new PropertyMember<T>[properties.Length];

            for (int i = 0; i < properties.Length; i++) {
                members[i] = new PropertyMember<T>(properties[i]);
            }

            return members;
        }

        /// <summary>
        /// Creates a collection of <see cref="PropertyMember"/> from the given <see cref="PropertyList{T}"/>.
        /// </summary>
        internal static PropertyMember<T>[] MembersFromPropertyList(PropertyList<T> propertyList)
        {
            PropertyMember<T>[] members = new PropertyMember<T>[propertyList.Properties.Length];

            for (int i = 0; i < propertyList.Properties.Length; i++) {
                members[i] = new PropertyMember<T>(propertyList.Properties[i]);
            }

            return members;
        }

        /// <summary>
        /// Property which this <see cref="Member"/> proxies.
        /// </summary>
        /// <remarks>
        /// Must be public so that types derived from <see cref="Mapper{TSource, TTarget}"/>
        /// can filter members if they override member resolution methods.
        /// </remarks>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Returns true if this member supports read operations.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return Property.CanRead && Property.GetGetMethod() != null;
            }
        }

        /// <summary>
        /// Returns true if this member supports write operations.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return Property.CanWrite && Property.GetSetMethod() != null;
            }
        }

        /// <summary>
        /// Name of the mapped member.
        /// </summary>
        public override string Name
        {
            get
            {
                return Property.Name;
            }
        }

        /// <summary>
        /// Runtime type of the member.
        /// </summary>
        public override Type Type
        {
            get
            {
                return Property.PropertyType;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="PropertyMember"/>.
        /// </summary>
        public PropertyMember(PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            Property = property;
        }

        /// <summary>
        /// Produces an expression which retrieves the relevant member value from the source.
        /// </summary>
        protected internal override Expression ResolveGetter(ParameterExpression source)
        {
            if (!CanRead) {
                throw new InvalidOperationException();
            }

            return Expression.MakeMemberAccess(source, Property);
        }

        /// <summary>
        /// Produces an expression which stores the value in the relevant member of the target.
        /// </summary>
        protected internal override Expression ResolveSetter(ParameterExpression target)
        {
            if (!CanWrite) {
                throw new InvalidOperationException();
            }

            return Expression.MakeMemberAccess(target, Property);
        }
    }
}