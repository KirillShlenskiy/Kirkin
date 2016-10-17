using System;
using System.Linq.Expressions;

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

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from object sources of the given type to various target types. 
        /// </summary>
        public PartiallyConfiguredMapperBuilder<TSource> FromPublicInstanceProperties<TSource>()
        {
            Member<TSource>[] sourceMembers = PropertyMember.PublicInstanceProperties<TSource>();

            return FromMembers(sourceMembers);
        }

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the specified members of the source type to various target types.
        /// </summary>
        public PartiallyConfiguredMapperBuilder<TSource> FromMembers<TSource>(Member<TSource>[] sourceMembers)
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
    }
}