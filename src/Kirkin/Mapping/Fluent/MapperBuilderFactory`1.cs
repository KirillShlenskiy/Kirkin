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
            Member<TTarget>[] targetMembers = PropertyMember<TTarget>.PublicInstanceProperties();

            return CreateAndConfigureBuilder<TTarget>(targetMembers);
        }

        /// <summary>
        /// Creates a <see cref="MapperBuilder{TSource, TTarget}"/> which defines a
        /// mapping from source to the specified properties of the given target type. 
        /// </summary>
        public MapperBuilder<TSource, TTarget> ToPropertyList<TTarget>(PropertyList<TTarget> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            Member<TTarget>[] targetMembers = PropertyMember<TTarget>.MembersFromPropertyList(propertyList);

            return CreateAndConfigureBuilder<TTarget>(targetMembers);
        }

        internal MapperBuilder<TSource, TTarget> ToProvider<TTarget>(IMemberListProvider<TTarget> targetMemberListProvider)
        {
            if (targetMemberListProvider == null) throw new ArgumentNullException(nameof(targetMemberListProvider));

            Member<TTarget>[] targetMembers = targetMemberListProvider.GetMembers();

            return CreateAndConfigureBuilder<TTarget>(targetMembers);
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