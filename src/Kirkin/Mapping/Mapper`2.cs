using System;
using System.Collections.Generic;

using Kirkin.Collections.Generic;
using Kirkin.Mapping.Engine.Compilers;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Type which performs mapping between objects of source and target types.
    /// </summary>
    public sealed class Mapper<TSource, TTarget>
        : IMapper<TSource, TTarget>
    {
        #region Static members

        /// <summary>
        /// <see cref="MappingCompiler{TSource, TTarget}"/> used
        /// by this and all other instances of the same type.
        /// </summary>
        private static readonly CachedMappingCompiler<TSource, TTarget> MappingCompiler
            = new CachedMappingCompiler<TSource, TTarget>();

        #endregion

        #region Fields and properties

        /// <summary>
        /// Cached compiled mapping delegate.
        /// </summary>
        /// <remarks>
        /// Non-thread-safe by design.
        /// Defined as <see cref="Func{TSource, TTarget, TTarget}"/> (as opposed to action)
        /// in order to support mapping scenarios where the target is a struct.
        /// </remarks>
        private Func<TSource, TTarget, TTarget> CompiledMapping;

        /// <summary>
        /// Backing field for <see cref="MemberMappings"/>.
        /// </summary>
        private readonly MemberMapping<TSource, TTarget>[] _memberMappings;

        /// <summary>
        /// Member mappings.
        /// </summary>
        public Vector<MemberMapping<TSource, TTarget>> MemberMappings
        {
            get
            {
                return new Vector<MemberMapping<TSource, TTarget>>(_memberMappings);
            }
        }

        /// <summary>
        /// Member mappings.
        /// </summary>
        IEnumerable<MemberMapping<TSource, TTarget>> IMapper<TSource, TTarget>.MemberMappings
        {
            get
            {
                return _memberMappings;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="Mapper{TSource, TTarget}"/> instance with the given configuration.
        /// </summary>
        internal Mapper(MemberMapping<TSource, TTarget>[] memberMappings)
        {
            _memberMappings = memberMappings;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// </summary>
        public TTarget Map(TSource source, TTarget target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            if (CompiledMapping == null) {
                CompiledMapping = MappingCompiler.CompileMapping(_memberMappings);
            }

            return CompiledMapping(source, target);
        }

        #endregion
    }
}