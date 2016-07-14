using System;
using System.Linq.Expressions;

using Kirkin.Mapping.Engine;
using Kirkin.Mapping.Engine.MemberMappings;

namespace Kirkin.Mapping
{
    partial class MapperBuilder<TSource, TTarget>
    {
        /// <summary>
        /// Member mapping configuration rewriter type.
        /// </summary>
        public abstract class TargetMemberConfigBase
        {
            /// <summary>
            /// Builder specified when this instance was created.
            /// </summary>
            protected readonly MapperBuilder<TSource, TTarget> MapperBuilder;

            /// <summary>
            /// <see cref="Member"/> configured by this instance.
            /// </summary>
            public Member Member { get; }

            /// <summary>
            /// Creates a new <see cref="TargetMemberConfigBase"/> instance.
            /// </summary>
            internal TargetMemberConfigBase(MapperBuilder<TSource, TTarget> mapperBuilder, Member member)
            {
                MapperBuilder = mapperBuilder;
                Member = member;
            }

            /// <summary>
            /// Configures the mapper to ignore this target member.
            /// Does not automatically ignore the matching source member.
            /// </summary>
            public void Ignore()
            {
                MapperBuilder.IgnoredTargetMembers.Add(Member);
                MapperBuilder.CustomTargetMappingFactories.Remove(Member);
            }

            /// <summary>
            /// Maps this target member to the source member with the given name.
            /// </summary>
            public void MapTo(string sourceMemberName)
            {
                Member sourceMember = MapperBuilder.SourceMember(sourceMemberName).Member;

                MapTo(sourceMember);
            }

            /// <summary>
            /// Maps this target member to the source member with the given name.
            /// </summary>
            public void MapTo(string sourceMemberName, StringComparer nameComparer)
            {
                Member sourceMember = MapperBuilder.SourceMember(sourceMemberName, nameComparer).Member;

                MapTo(sourceMember);
            }

            /// <summary>
            /// Maps this target member to the given source member.
            /// </summary>
            private void MapTo(Member sourceMember)
            {
                MapperBuilder.IgnoredTargetMembers.Remove(Member);
                MapperBuilder.CustomTargetMappingFactories[Member] = builder => new DefaultMemberMapping<TSource, TTarget>(sourceMember, Member, builder.NullableBehaviour);
            }

            /// <summary>
            /// Resets member configuration so that this target member
            /// is auto-mapped to a source member with a matching name.
            /// </summary>
            public void Reset()
            {
                MapperBuilder.IgnoredTargetMembers.Remove(Member);
                MapperBuilder.CustomTargetMappingFactories.Remove(Member);
            }

            #region Experimental

            /// <summary>
            /// Configures the mapper to ignore this target member,
            /// optionally ignoring the matching source member too.
            /// </summary>
            internal void Ignore(bool ignoreMatchingSource)
            {
                Ignore();

                if (ignoreMatchingSource)
                {
                    foreach (Member sourceMember in MapperBuilder.SourceMembers)
                    {
                        if (MapperBuilder.MemberNameComparer.Equals(sourceMember.Name, Member.Name)) {
                            MapperBuilder.IgnoredSourceMembers.Add(sourceMember);
                        }
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Non-generic member mapping configuration rewriter type.
        /// </summary>
        public sealed class TargetMemberConfig : TargetMemberConfigBase
        {
            /// <summary>
            /// Creates a new <see cref="TargetMemberConfig"/> instance.
            /// </summary>
            internal TargetMemberConfig(MapperBuilder<TSource, TTarget> mapperBuilder, Member member)
                : base(mapperBuilder, member)
            {
            }

            /// <summary>
            /// Registers the expression to be executed when the current member is mapped.
            /// </summary>
            public void MapTo<TValue>(Expression<Func<TSource, TValue>> sourceValueSelector)
            {
                MapperBuilder.IgnoredTargetMembers.Remove(Member);
                MapperBuilder.CustomTargetMappingFactories[Member] = builder => new ExpressionMemberMapping<TSource, TTarget, TValue>(Member, sourceValueSelector);
            }

            /// <summary>
            /// Registers the delegate to be invoked when the current member is mapped.
            /// </summary>
            public void MapWithDelegate<TValue>(Func<TSource, TValue> sourceValueSelector)
            {
                MapperBuilder.IgnoredTargetMembers.Remove(Member);
                MapperBuilder.CustomTargetMappingFactories[Member] = builder => new DelegateMemberMapping<TSource, TTarget, TValue>(Member, sourceValueSelector);
            }
        }

        /// <summary>
        /// Generic member mapping configuration rewriter type.
        /// </summary>
        public sealed class TargetMemberConfig<TValue> : TargetMemberConfigBase
        {
            /// <summary>
            /// Creates a new <see cref="TargetMemberConfig{TValue}"/> instance.
            /// </summary>
            internal TargetMemberConfig(MapperBuilder<TSource, TTarget> mapperBuilder, Member member)
                : base(mapperBuilder, member)
            {
            }

            /// <summary>
            /// Registers the expression to be executed when the current member is mapped.
            /// </summary>
            public void MapTo(Expression<Func<TSource, TValue>> sourceValueSelector)
            {
                MapperBuilder.IgnoredTargetMembers.Remove(Member);
                MapperBuilder.CustomTargetMappingFactories[Member] = builder => new ExpressionMemberMapping<TSource, TTarget, TValue>(Member, sourceValueSelector);
            }

            /// <summary>
            /// Registers the delegate to be invoked when the current member is mapped.
            /// </summary>
            public void MapWithDelegate(Func<TSource, TValue> sourceValueSelector)
            {
                MapperBuilder.IgnoredTargetMembers.Remove(Member);
                MapperBuilder.CustomTargetMappingFactories[Member] = builder => new DelegateMemberMapping<TSource, TTarget, TValue>(Member, sourceValueSelector);
            }
        }
    }
}