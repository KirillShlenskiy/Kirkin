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

        public MapperBuilder<TSource, TTarget> ToObject<TTarget>()
        {
            return CreateAndConfigureBuilder<TTarget>(
                PropertyMember.PublicInstanceProperties<TTarget>()
            );
        }

        protected virtual MapperBuilder<TSource, TTarget> CreateAndConfigureBuilder<TTarget>(Member[] targetMembers)
        {
            return new MapperBuilder<TSource, TTarget>(SourceMembers, targetMembers);
        }
    }
}