namespace Kirkin.Mapping
{
    /// <summary>
    /// Wraps the default <see cref="IMapper{TSource, TTarget}"/>
    /// instance which uses <see cref="MappingMode.Strict"/>.
    /// </summary>
    internal static class StrictMapper<TSource, TTarget>
    {
        private static Mapper<TSource, TTarget> _default;

        /// <summary>
        /// Default <see cref="Mapper{TSource, TTarget}"/> instance.
        /// *DO NOT* mutate this instance.
        /// </summary>
        internal static Mapper<TSource, TTarget> Default
        {
            get
            {
                // Lazy initialized to avoid a TypeInitializationException in case of a bad mapping,
                if (_default == null) {
                    _default = CreateMapper();
                }

                return _default;
            }
        }

        /// <summary>
        /// Creates a new mapper instance using appropriate member mapping rules.
        /// </summary>
        private static Mapper<TSource, TTarget> CreateMapper()
        {
            MapperConfig<TSource, TTarget> config = new MapperConfig<TSource, TTarget> {
                MappingMode = MappingMode.Strict
            };

            return new Mapper<TSource, TTarget>(config.ProduceValidMemberMappings());
        }
    }

    /// <summary>
    /// Wraps the default <see cref="IMapper{TSource, TTarget}"/>
    /// instance which uses <see cref="MappingMode.Relaxed"/>.
    /// </summary>
    internal static class RelaxedMapper<TSource, TTarget>
    {
        private static Mapper<TSource, TTarget> _default;

        /// <summary>
        /// Default <see cref="Mapper{TSource, TTarget}"/> instance.
        /// *DO NOT* mutate this instance.
        /// </summary>
        internal static Mapper<TSource, TTarget> Default
        {
            get
            {
                // Lazy initialized to avoid a TypeInitializationException in case of a bad mapping,
                if (_default == null) {
                    _default = CreateMapper();
                }

                return _default;
            }
        }

        /// <summary>
        /// Creates a new mapper instance using appropriate member mapping rules.
        /// </summary>
        private static Mapper<TSource, TTarget> CreateMapper()
        {
            MapperConfig<TSource, TTarget> config = new MapperConfig<TSource, TTarget> {
                MappingMode = MappingMode.Relaxed
            };

            return new Mapper<TSource, TTarget>(config.ProduceValidMemberMappings());
        }
    }

    /// <summary>
    /// Wraps the default <see cref="IMapper{TSource, TTarget}"/>
    /// instance which uses <see cref="MappingMode.AllSourceMembers"/>.
    /// </summary>
    internal static class AllSourceMembersMapper<TSource, TTarget>
    {
        private static Mapper<TSource, TTarget> _default;

        /// <summary>
        /// Default <see cref="Mapper{TSource, TTarget}"/> instance.
        /// *DO NOT* mutate this instance.
        /// </summary>
        internal static Mapper<TSource, TTarget> Default
        {
            get
            {
                // Lazy initialized to avoid a TypeInitializationException in case of a bad mapping,
                if (_default == null) {
                    _default = CreateMapper();
                }

                return _default;
            }
        }

        /// <summary>
        /// Creates a new mapper instance using appropriate member mapping rules.
        /// </summary>
        private static Mapper<TSource, TTarget> CreateMapper()
        {
            MapperConfig<TSource, TTarget> config = new MapperConfig<TSource, TTarget> {
                MappingMode = MappingMode.AllSourceMembers
            };

            return new Mapper<TSource, TTarget>(config.ProduceValidMemberMappings());
        }
    }

    /// <summary>
    /// Wraps the default <see cref="IMapper{TSource, TTarget}"/>
    /// instance which uses <see cref="MappingMode.AllTargetMembers"/>.
    /// </summary>
    internal static class AllTargetMembersMapper<TSource, TTarget>
    {
        private static Mapper<TSource, TTarget> _default;

        /// <summary>
        /// Default <see cref="Mapper{TSource, TTarget}"/> instance.
        /// *DO NOT* mutate this instance.
        /// </summary>
        internal static Mapper<TSource, TTarget> Default
        {
            get
            {
                // Lazy initialized to avoid a TypeInitializationException in case of a bad mapping,
                if (_default == null) {
                    _default = CreateMapper();
                }

                return _default;
            }
        }

        /// <summary>
        /// Creates a new mapper instance using appropriate member mapping rules.
        /// </summary>
        private static Mapper<TSource, TTarget> CreateMapper()
        {
            MapperConfig<TSource, TTarget> config = new MapperConfig<TSource, TTarget> {
                MappingMode = MappingMode.AllTargetMembers
            };

            return new Mapper<TSource, TTarget>(config.ProduceValidMemberMappings());
        }
    }
}