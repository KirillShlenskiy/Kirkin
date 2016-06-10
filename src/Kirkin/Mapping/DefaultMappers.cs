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
                if (_default == null)
                {
                    MapperConfig<TSource, TTarget> config = new MapperConfig<TSource, TTarget> {
                        MappingMode = MappingMode.Strict
                    };

                    _default = new Mapper<TSource, TTarget>(config.ProduceValidMemberMappings());
                }

                return _default;
            }
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
                if (_default == null)
                {
                    MapperConfig<TSource, TTarget> config = new MapperConfig<TSource, TTarget> {
                        MappingMode = MappingMode.Relaxed
                    };

                    _default = new Mapper<TSource, TTarget>(config.ProduceValidMemberMappings());
                }

                return _default;
            }
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
                if (_default == null)
                {
                    MapperConfig<TSource, TTarget> config = new MapperConfig<TSource, TTarget> {
                        MappingMode = MappingMode.AllSourceMembers
                    };

                    _default = new Mapper<TSource, TTarget>(config.ProduceValidMemberMappings());
                }

                return _default;
            }
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
                if (_default == null)
                {
                    MapperConfig<TSource, TTarget> config = new MapperConfig<TSource, TTarget> {
                        MappingMode = MappingMode.AllTargetMembers
                    };

                    _default = new Mapper<TSource, TTarget>(config.ProduceValidMemberMappings());
                }

                return _default;
            }
        }
    }
}