using System;
using System.Linq.Expressions;

using Kirkin.Reflection;
using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping.Fluent
{
    /// <summary>
    /// <see cref="MapperBuilder{TSource, TTarget}"/> factory methods.
    /// </summary>
    public sealed class MapperBuilderFactory
    {
        internal MapperBuilderFactory()
        {
        }

        internal MapperBuilderFactory<TSource> From<TSource>(IMemberListProvider<TSource> memberListProvider)
        {
            if (memberListProvider == null) throw new ArgumentNullException(nameof(memberListProvider));

            return new MapperBuilderFactory<TSource>(memberListProvider.GetMembers());
        }

        /// <summary>
        /// Expression-based <see cref="MapperBuilder{TSource, TTarget}"/> factory placeholder.
        /// </summary>
        internal MapperBuilder<TSource, TTarget> FromExpression<TSource, TTarget>(Expression<Func<TSource, TTarget>> expression)
        {
            throw new NotImplementedException(); // TODO.
        }

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from object sources of the given type to various target types. 
        /// </summary>
        public MapperBuilderFactory<TSource> FromType<TSource>()
        {
            Member[] sourceMembers = PropertyMember.PublicInstanceProperties<TSource>();

            return new MapperBuilderFactory<TSource>(sourceMembers);
        }

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the specified properties of the given type to various target types.
        /// </summary>
        public MapperBuilderFactory<TSource> FromPropertyList<TSource>(PropertyList<TSource> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            Member[] sourceMembers = PropertyMember.MembersFromPropertyList(propertyList);

            return new MapperBuilderFactory<TSource>(sourceMembers);
        }
    }
}