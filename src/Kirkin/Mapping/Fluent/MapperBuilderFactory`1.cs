using System;

using Kirkin.Mapping.Engine;
using Kirkin.Reflection;

namespace Kirkin.Mapping.Fluent
{
    /// <summary>
    /// Partially configure mapper builder factory type.
    /// Participates in fluent mapper builder construction.
    /// </summary>
    public class MapperBuilderFactory<TSource>
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
        public MapperBuilder<TSource, TTarget> ToType<TTarget>()
        {
            Member<TTarget>[] targetMembers = PropertyMember.PublicInstanceProperties<TTarget>();

            return CreateAndConfigureBuilder(targetMembers);
        }

        /// <summary>
        /// Creates a <see cref="MapperBuilder{TSource, TTarget}"/> which defines a
        /// mapping from source to the specified properties of the given target type. 
        /// </summary>
        public MapperBuilder<TSource, TTarget> ToPropertyList<TTarget>(PropertyList<TTarget> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            Member<TTarget>[] targetMembers = PropertyMember.MembersFromPropertyList(propertyList);

            return CreateAndConfigureBuilder(targetMembers);
        }

        internal MapperBuilder<TSource, TTarget> To<TTarget>(Member<TTarget>[] targetMembers)
        {
            if (targetMembers == null) throw new ArgumentNullException(nameof(targetMembers));

            return CreateAndConfigureBuilder(targetMembers);
        }

        /// <summary>
        /// Core <see cref="MapperBuilder{TSource, TTarget}"/> factory method.
        /// </summary>
        protected virtual MapperBuilder<TSource, TTarget> CreateAndConfigureBuilder<TTarget>(Member<TTarget>[] targetMembers)
        {
            return new MapperBuilder<TSource, TTarget>(SourceMembers, targetMembers);
        }
    }
}