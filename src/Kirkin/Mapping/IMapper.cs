using System.Collections.Generic;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Performs mapping between objects of source and target types.
    /// </summary>
    internal interface IMapper<TSource, TTarget> // To be retired.
    {
        /// <summary>
        /// Member mappings.
        /// </summary>
        IEnumerable<MemberMapping<TSource, TTarget>> MemberMappings { get; }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// </summary>
        TTarget Map(TSource source, TTarget target);
    }
}