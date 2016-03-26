using System;
using System.Collections.Generic;

using Kirkin.Mapping.Engine;
using Kirkin.Mapping.Engine.Compilers;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Static <see cref="IMapper{TSource, TTarget}" /> proxy and factory methods.
    /// </summary>
    public static class Mapper
    {
        #region Factory

        /// <summary>
        /// Creates and configures a new <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static IMapper<TSource, TTarget> CreateMapper<TSource, TTarget>()
        {
            return Mapper<TSource, TTarget>.Default;
        }

        /// <summary>
        /// Creates and configures a new <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static IMapper<TSource, TTarget> CreateMapper<TSource, TTarget>(Action<MapperConfig<TSource, TTarget>> configAction)
        {
            if (configAction == null) throw new ArgumentNullException(nameof(configAction));

            MapperConfig<TSource, TTarget> config = new MapperConfig<TSource, TTarget>();

            configAction(config);

            return new Mapper<TSource, TTarget>(config.ProduceValidMemberMappings());
        }

        /// <summary>
        /// Creates a new <see cref="IMapper{TSource, TTarget}"/> instance with the given configuration.
        /// </summary>
        public static IMapper<TSource, TTarget> CreateMapper<TSource, TTarget>(MapperConfig<TSource, TTarget> config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            return new Mapper<TSource, TTarget>(config.ProduceValidMemberMappings());
        }

        #endregion

        #region Convenience proxies

        /// <summary>
        /// Creates a shallow clone of the given object.
        /// </summary>
        public static T Clone<T>(T source)
            where T : new()
        {
            return Mapper<T, T>.Default.Map(source, new T());
        }

        /// <summary>
        /// Creates a new target instance, executes mapping from
        /// source to target and returns the newly created target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget DynamicMap<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            return DynamicMap(source, new TTarget());
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget DynamicMap<TSource, TTarget>(TSource source, TTarget target)
        {
            return Mapper<TSource, TTarget>.Default.Map(source, target);
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Target instance must have the same type as source, or be derived from source.
        /// </summary>
        public static TTarget Map<TSource, TTarget>(TSource source, TTarget target)
            where TTarget : TSource
        {
            // Treat target as TSource so as to prevent the default
            // mapping from failing due to unmapped target members.
            return (TTarget)DynamicMap<TSource, TSource>(source, target);
        }

        #endregion
    }

    /// <summary>
    /// Constrained <see cref="IMapper{TSource, TTarget}" /> extensions.
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Creates a new target instance, executes mapping from source
        /// to target and returns the newly created target instance.
        /// </summary>
        public static TTarget Map<TSource, TTarget>(this IMapper<TSource, TTarget> mapper, TSource source)
            where TTarget : new()
        {
            return mapper.Map(source, new TTarget());
        }
    }

    /// <summary>
    /// Type which performs mapping between objects of source and target types.
    /// </summary>
    internal sealed class Mapper<TSource, TTarget>
        : IMapper<TSource, TTarget>
    {
        #region Static members

        private static Mapper<TSource, TTarget> _default;

        /// <summary>
        /// Default <see cref="Mapper{TSource, TTarget}"/> instance.
        /// *DO NOT* mutate this instance.
        /// </summary>
        /// <remarks>
        /// Internal to encourage consumers to use the safer static <see cref="Mapper"/> methods.
        /// </remarks>
        internal static Mapper<TSource, TTarget> Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new Mapper<TSource, TTarget>(
                        new MapperConfig<TSource, TTarget>().ProduceValidMemberMappings()
                    );
                }

                return _default;
            }
        }

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

        private readonly MemberMapping<TSource, TTarget>[] _memberMappings;

        /// <summary>
        /// Member mappings.
        /// </summary>
        public IEnumerable<MemberMapping<TSource, TTarget>> MemberMappings
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