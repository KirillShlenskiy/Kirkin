using System;
using System.Collections.Concurrent;

namespace Kirkin.Mapping.Engine.Compilers
{
    /// <summary>
    /// Type responsible for producing and caching compiled mapping delegates.
    /// </summary>
    internal sealed class CachedMappingCompiler<TSource, TTarget> : MappingCompiler<TSource, TTarget>
    {
        private readonly ConcurrentDictionary<MemberMappingCollection<TSource, TTarget>, Func<TSource, TTarget, TTarget>> CompiledMappings
            = new ConcurrentDictionary<MemberMappingCollection<TSource, TTarget>, Func<TSource, TTarget, TTarget>>();

        /// <summary>
        /// Returns a compiled delegate which performs the mapping from source to target.
        /// </summary>
        public override Func<TSource, TTarget, TTarget> CompileMapping(MemberMapping<TSource, TTarget>[] memberMappings)
        {
            MemberMappingCollection<TSource, TTarget> memberMappingCollection = new MemberMappingCollection<TSource, TTarget>(memberMappings);
            Func<TSource, TTarget, TTarget> compiledMapping;

            if (!CompiledMappings.TryGetValue(memberMappingCollection, out compiledMapping))
            {
                compiledMapping = base.CompileMapping(memberMappings);

                if (!CompiledMappings.TryAdd(memberMappingCollection, compiledMapping)) {
                    return CompiledMappings[memberMappingCollection]; // Add preempted. Return most up-to-date value.
                }
            }

            return compiledMapping;
        }
    }
}