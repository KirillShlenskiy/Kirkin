using System;

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
            return StrictMapper<TSource, TTarget>.Default;
        }

        /// <summary>
        /// Creates and configures a new <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static IMapper<TSource, TTarget> CreateMapper<TSource, TTarget>(Action<MapperBuilder<TSource, TTarget>> configAction)
        {
            if (configAction == null) throw new ArgumentNullException(nameof(configAction));

            MapperBuilder<TSource, TTarget> builder = new MapperBuilder<TSource, TTarget>();

            configAction(builder);

            return builder.BuildMapper();
        }

        #endregion

        #region Same/derived type mapping

        /// <summary>
        /// Creates a shallow clone of the given object.
        /// </summary>
        public static T Clone<T>(T source)
            where T : new()
        {
            return RelaxedMapper<T, T>.Default.Map(source, new T());
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
            return (TTarget)RelaxedMapper<TSource, TSource>.Default.Map(source, target);
        }

        #endregion

        #region MappingMode shorthands

        /// <summary>
        /// Creates a new target instance, executes mapping from
        /// source to target and returns the newly created target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapAllSourceMembers<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            return MapAllSourceMembers(source, new TTarget());
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapAllSourceMembers<TSource, TTarget>(TSource source, TTarget target)
        {
            return AllSourceMembersMapper<TSource, TTarget>.Default.Map(source, target);
        }

        /// <summary>
        /// Creates a new target instance, executes mapping from
        /// source to target and returns the newly created target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapAllTargetMembers<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            return MapAllTargetMembers(source, new TTarget());
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapAllTargetMembers<TSource, TTarget>(TSource source, TTarget target)
        {
            return AllTargetMembersMapper<TSource, TTarget>.Default.Map(source, target);
        }

        /// <summary>
        /// Creates a new target instance, executes mapping from
        /// source to target and returns the newly created target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapRelaxed<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            return MapRelaxed(source, new TTarget());
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapRelaxed<TSource, TTarget>(TSource source, TTarget target)
        {
            return RelaxedMapper<TSource, TTarget>.Default.Map(source, target);
        }

        /// <summary>
        /// Creates a new target instance, executes mapping from
        /// source to target and returns the newly created target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapStrict<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            return MapStrict(source, new TTarget());
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapStrict<TSource, TTarget>(TSource source, TTarget target)
        {
            return StrictMapper<TSource, TTarget>.Default.Map(source, target);
        }

        #endregion
    }
}