using System;
using System.Collections.Generic;

using Kirkin.Mapping.Engine;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Performs mapping between objects of source and target types.
    /// </summary>
    [Obsolete("Use the concrete Mapper{TSource, TTarget} type instead.")]
    public interface IMapper<TSource, TTarget>
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