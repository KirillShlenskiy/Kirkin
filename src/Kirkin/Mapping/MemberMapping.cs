using System.Linq.Expressions;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Describes a mapping between two members.
    /// </summary>
    public abstract class MemberMapping<TSource, TTarget>
    {
        /// <summary>
        /// Mapping target member.
        /// </summary>
        public Member<TTarget> TargetMember { get; }

        /// <summary>
        /// Creates a new instance of <see cref="MemberMapping{TSource, TTarget}"/>.
        /// </summary>
        internal MemberMapping(Member<TTarget> targetMember)
        {
            TargetMember = targetMember;
        }

        /// <summary>
        /// When overridden in a derived class, produces an
        /// expression which resolves the source value.
        /// </summary>
        internal abstract Expression GetSourceValueExpression(ParameterExpression sourceParam);
    }
}