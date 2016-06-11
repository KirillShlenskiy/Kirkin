using System;

using Kirkin.Reflection;

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

        /// <summary>
        /// Creates a new <see cref="IMapper{TSource, TTarget}"/> instance with the
        /// same source and target type, mapping all the properties in the given list.
        /// </summary>
        public static IMapper<T, T> CreateMapper<T>(PropertyList<T> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            MapperConfig<T, T> config = new MapperConfig<T, T> {
                MappingMode = MappingMode.Relaxed // No need to validate mapping.
            };

            config.IgnoreAllTargetMembers();

            foreach (IPropertyAccessor propertyAccessor in propertyList.PropertyAccessors) {
                config.TargetMember(propertyAccessor.Property.Name).Reset();
            }

            return new Mapper<T, T>(config.ProduceValidMemberMappings());
        }

        #endregion

        #region Dynamic mapping proxies

        /// <summary>
        /// Creates a new target instance, executes mapping from
        /// source to target and returns the newly created target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        [Obsolete("Use MapStrict.")]
        public static TTarget DynamicMap<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            return MapStrict<TSource, TTarget>(source);
        }

        /// <summary>
        /// Executes mapping from source to target and returns the target instance.
        /// Uses the default cached <see cref="IMapper{TSource, TTarget}"/> instance.
        /// </summary>
        [Obsolete("Use MapStrict.")]
        public static TTarget DynamicMap<TSource, TTarget>(TSource source, TTarget target)
        {
            return MapStrict(source, target);
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
}