using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping
{
    partial class MapperConfig<TSource, TTarget>
    {
        /// <summary>
        /// Member mapping configuration rewriter type.
        /// </summary>
        public struct SourceMemberConfig
        {
            /// <summary>
            /// Mapping configuration specified when this instance was created.
            /// </summary>
            private readonly MapperConfig<TSource, TTarget> MapperConfig;

            /// <summary>
            /// <see cref="Member"/> configured by this instance.
            /// </summary>
            public Member Member { get; }

            /// <summary>
            /// Creates a new <see cref="SourceMemberConfig"/> instance.
            /// </summary>
            internal SourceMemberConfig(MapperConfig<TSource, TTarget> mapperConfig, Member member)
            {
                MapperConfig = mapperConfig;
                Member = member;
            }

            /// <summary>
            /// Configures the mapper to ignore this source member.
            /// Does not automatically ignore the matching target member.
            /// </summary>
            public void Ignore()
            {
                MapperConfig.IgnoredSourceMembers.Add(Member);
            }

            /// <summary>
            /// Resets member configuration so that this source member
            /// is auto-mapped to a target member with a matching name.
            /// </summary>
            public void Reset()
            {
                MapperConfig.IgnoredSourceMembers.Remove(Member);
            }
        }
    }
}