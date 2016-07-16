using System;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping.Fluent
{
    /// <summary>
    /// Partially configure mapper builder factory type.
    /// Participates in fluent mapper builder construction.
    /// </summary>
    public sealed class MapperBuilderFactory<TSource> // Cannot use struct as we want the constructor to be hidden.
    {
        private readonly Member<TSource>[] SourceMembers;

        internal MapperBuilderFactory(Member<TSource>[] sourceMembers)
        {
            if (sourceMembers == null) throw new ArgumentNullException(nameof(sourceMembers));

            SourceMembers = sourceMembers;
        }

        /// <summary>
        /// Creates a <see cref="MapperBuilder{TSource, TTarget}"/> which
        /// configures mapping from source to an object of the given type.
        /// </summary>
        public MapperBuilder<TSource, TTarget> To<TTarget>()
        {
            Member<TTarget>[] targetMembers = PropertyMember.PublicInstanceProperties<TTarget>();

            return To(targetMembers);
        }

        /// <summary>
        /// Creates a <see cref="MapperBuilder{TSource, TTarget}"/> which
        /// configures mapping from source to the given members of the target type.
        /// </summary>
        internal MapperBuilder<TSource, TTarget> To<TTarget>(Member<TTarget>[] targetMembers)
        {
            if (targetMembers == null) throw new ArgumentNullException(nameof(targetMembers));

            return new MapperBuilder<TSource, TTarget>(SourceMembers, targetMembers);
        }

        ///// <summary>
        ///// Creates a <see cref="MapperBuilder{TSource, TTarget}"/> which defines a
        ///// mapping from source to the specified members of the given target type. 
        ///// </summary>
        //public MapperBuilder<TSource, TTarget> ToPropertyList<TTarget>(PropertyList<TTarget> propertyList)
        //{
        //    if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

        //    Member<TTarget>[] targetMembers = PropertyMember.MembersFromPropertyList(propertyList);

        //    return To(targetMembers);
        //}
    }
}