using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping
{
    partial class MapperBuilder<TSource, TTarget>
    {
        /// <summary>
        /// Member mapping configuration rewriter type.
        /// </summary>
        public struct SourceMemberConfig
        {
            /// <summary>
            /// Mapping configuration specified when this instance was created.
            /// </summary>
            private readonly MapperBuilder<TSource, TTarget> MapperBuilder;

            /// <summary>
            /// <see cref="Member"/> configured by this instance.
            /// </summary>
            public Member<TSource> Member { get; }

            /// <summary>
            /// Creates a new <see cref="SourceMemberConfig"/> instance.
            /// </summary>
            internal SourceMemberConfig(MapperBuilder<TSource, TTarget> mapperBuilder, Member<TSource> member)
            {
                MapperBuilder = mapperBuilder;
                Member = member;
            }

            /// <summary>
            /// Configures the mapper to ignore this source member.
            /// Does not automatically ignore the matching target member.
            /// </summary>
            public void Ignore()
            {
                MapperBuilder.IgnoredSourceMembers.Add(Member);
            }

            /// <summary>
            /// Resets member configuration so that this source member
            /// is auto-mapped to a target member with a matching name.
            /// </summary>
            public void Reset()
            {
                MapperBuilder.IgnoredSourceMembers.Remove(Member);
            }
        }
    }
}