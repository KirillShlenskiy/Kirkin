using System;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping
{
    internal struct MapperBuilderFactory<TSource>
    {
        private readonly Member[] SourceMembers;

        internal MapperBuilderFactory(Member[] sourceMembers) // Pass in Func<MapperBuilder<TSource, TFactory>> where TSource is pre-set?
        {
            if (sourceMembers == null) throw new ArgumentNullException(nameof(sourceMembers));

            SourceMembers = sourceMembers;
        }

        public MapperBuilder<TSource, TTarget> ToObject<TTarget>()
        {
            Member[] targetMembers = PropertyMember.PublicInstanceProperties<TTarget>();

            return new MapperBuilder<TSource, TTarget>(SourceMembers, targetMembers);
        }
    }
}