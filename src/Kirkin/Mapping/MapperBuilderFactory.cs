using System;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Partially configure mapper builder factory type.
    /// Participates in fluent mapper builder construction.
    /// </summary>
    internal class MapperBuilderFactory<TSource>
    {
        private readonly Member[] SourceMembers;

        internal MapperBuilderFactory(Member[] sourceMembers)
        {
            if (sourceMembers == null) throw new ArgumentNullException(nameof(sourceMembers));

            SourceMembers = sourceMembers;
        }

        /// <summary>
        /// Creates a <see cref="MapperBuilder{TSource, TTarget}"/> which
        /// configures mapping from source to an object of the given type.
        /// </summary>
        public MapperBuilder<TSource, TTarget> ToObject<TTarget>()
        {
            Member[] targetMembers = PropertyMember.PublicInstanceProperties<TTarget>();

            return CreateAndConfigureBuilder<TTarget>(targetMembers);
        }

        /// <summary>
        /// Core <see cref="MapperBuilder{TSource, TTarget}"/> factory method.
        /// </summary>
        protected virtual MapperBuilder<TSource, TTarget> CreateAndConfigureBuilder<TTarget>(Member[] targetMembers)
        {
            return new MapperBuilder<TSource, TTarget>(SourceMembers, targetMembers);
        }
    }
}