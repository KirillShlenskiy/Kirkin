using System;
using System.Linq.Expressions;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping.Fluent
{
    /// <summary>
    /// <see cref="MapperBuilder{TSource, TTarget}"/> factory methods.
    /// </summary>
    public sealed class MapperBuilderFactory
    {
        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from object sources of the given type to various target types. 
        /// </summary>
        public PartiallyConfiguredMapperBuilder<TSource> From<TSource>()
        {
            Member<TSource>[] sourceMembers = PropertyMember.PublicInstanceProperties<TSource>();

            return From(sourceMembers);
        }

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the specified members of the source type to various target types.
        /// </summary>
        public PartiallyConfiguredMapperBuilder<TSource> From<TSource>(Member<TSource>[] sourceMembers)
        {
            if (sourceMembers == null) throw new ArgumentNullException(nameof(sourceMembers));

            return new PartiallyConfiguredMapperBuilder<TSource>(sourceMembers);
        }

        /// <summary>
        /// Expression-based <see cref="MapperBuilder{TSource, TTarget}"/> factory placeholder.
        /// </summary>
        internal MapperBuilder<TSource, TTarget> FromExpression<TSource, TTarget>(Expression<Func<TSource, TTarget>> expression)
        {
            throw new NotImplementedException(); // TODO.
        }

        ///// <summary>
        ///// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        ///// instances mapping from the specified properties of the given type to various target types.
        ///// </summary>
        //public MapperBuilderFactory<TSource> FromPropertyList<TSource>(PropertyList<TSource> propertyList)
        //{
        //    if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

        //    Member<TSource>[] sourceMembers = PropertyMember.MembersFromPropertyList(propertyList);

        //    return new MapperBuilderFactory<TSource>(sourceMembers);
        //}
    }
}