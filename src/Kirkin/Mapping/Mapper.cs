using Kirkin.Mapping.Fluent;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Static <see cref="Mapper{TSource, TTarget}" /> proxy and factory methods.
    /// </summary>
    public static class Mapper
    {
        #region Builder factory

        /// <summary>
        /// Fluent <see cref="MapperBuilder{TSource, TTarget}"/> factory singleton.
        /// </summary>
        public static MapperBuilderFactory Builder { get; } = new MapperBuilderFactory();

        #endregion

        #region Same/derived type mapping

        /// <summary>
        /// Creates a shallow clone of the given object.
        /// </summary>
        public static T Clone<T>(T source)
            where T : new()
        {
            return DefaultMappers.RelaxedMapper<T, T>.Instance.Map(source, FastActivator.CreateInstance<T>());
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
            return (TTarget)DefaultMappers.RelaxedMapper<TSource, TSource>.Instance.Map(source, target);
        }

        #endregion

        #region MappingMode shorthands

        /// <summary>
        /// Creates a new target instance, executes mapping from
        /// source to target and returns the newly created target instance.
        /// Uses the default cached <see cref="Mapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapAllSourceMembers<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            return MapAllSourceMembers(source, FastActivator.CreateInstance<TTarget>());
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Uses the default cached <see cref="Mapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapAllSourceMembers<TSource, TTarget>(TSource source, TTarget target)
        {
            return DefaultMappers.AllSourceMembersMapper<TSource, TTarget>.Instance.Map(source, target);
        }

        /// <summary>
        /// Creates a new target instance, executes mapping from
        /// source to target and returns the newly created target instance.
        /// Uses the default cached <see cref="Mapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapAllTargetMembers<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            return MapAllTargetMembers(source, FastActivator.CreateInstance<TTarget>());
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Uses the default cached <see cref="Mapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapAllTargetMembers<TSource, TTarget>(TSource source, TTarget target)
        {
            return DefaultMappers.AllTargetMembersMapper<TSource, TTarget>.Instance.Map(source, target);
        }

        /// <summary>
        /// Creates a new target instance, executes mapping from
        /// source to target and returns the newly created target instance.
        /// Uses the default cached <see cref="Mapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapRelaxed<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            return MapRelaxed(source, FastActivator.CreateInstance<TTarget>());
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Uses the default cached <see cref="Mapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapRelaxed<TSource, TTarget>(TSource source, TTarget target)
        {
            return DefaultMappers.RelaxedMapper<TSource, TTarget>.Instance.Map(source, target);
        }

        /// <summary>
        /// Creates a new target instance, executes mapping from
        /// source to target and returns the newly created target instance.
        /// Uses the default cached <see cref="Mapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapStrict<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            return MapStrict(source, FastActivator.CreateInstance<TTarget>());
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Uses the default cached <see cref="Mapper{TSource, TTarget}"/> instance.
        /// </summary>
        public static TTarget MapStrict<TSource, TTarget>(TSource source, TTarget target)
        {
            return DefaultMappers.StrictMapper<TSource, TTarget>.Instance.Map(source, target);
        }

        #endregion
    }
}