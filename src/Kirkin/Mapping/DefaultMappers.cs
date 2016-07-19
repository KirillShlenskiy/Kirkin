namespace Kirkin.Mapping
{
    /// <summary>
    /// Container type for default mapper instances.
    /// </summary>
    internal static class DefaultMappers
    {
        /// <summary>
        /// Wraps the default <see cref="Mapper{TSource, TTarget}"/>
        /// instance which does not allow unmapped source or target members.
        /// </summary>
        internal static class StrictMapper<TSource, TTarget>
        {
            // Lazy initialized to avoid a TypeInitializationException in case of a bad mapping,
            private static Mapper<TSource, TTarget> _instance;

            /// <summary>
            /// Default <see cref="Mapper{TSource, TTarget}"/> instance.
            /// *DO NOT* mutate this instance.
            /// </summary>
            internal static Mapper<TSource, TTarget> Instance
            {
                get
                {
                    if (_instance == null) {
                        _instance = CreateMapper();
                    }

                    return _instance;
                }
            }

            /// <summary>
            /// Creates a new mapper instance using appropriate member mapping rules.
            /// </summary>
            private static Mapper<TSource, TTarget> CreateMapper()
            {
                MapperBuilder<TSource, TTarget> config = new MapperBuilder<TSource, TTarget> {
                    AllowUnmappedSourceMembers = false,
                    AllowUnmappedTargetMembers = false
                };

                return config.BuildMapper();
            }
        }

        /// <summary>
        /// Wraps the default <see cref="Mapper{TSource, TTarget}"/>
        /// instance which allows unmapped source and target members.
        /// </summary>
        internal static class RelaxedMapper<TSource, TTarget>
        {
            // Lazy initialized to avoid a TypeInitializationException in case of a bad mapping,
            private static Mapper<TSource, TTarget> _instance;

            /// <summary>
            /// Default <see cref="Mapper{TSource, TTarget}"/> instance.
            /// *DO NOT* mutate this instance.
            /// </summary>
            internal static Mapper<TSource, TTarget> Instance
            {
                get
                {
                    if (_instance == null) {
                        _instance = CreateMapper();
                    }

                    return _instance;
                }
            }

            /// <summary>
            /// Creates a new mapper instance using appropriate member mapping rules.
            /// </summary>
            private static Mapper<TSource, TTarget> CreateMapper()
            {
                MapperBuilder<TSource, TTarget> config = new MapperBuilder<TSource, TTarget> {
                    AllowUnmappedSourceMembers = true,
                    AllowUnmappedTargetMembers = true
                };

                return config.BuildMapper();
            }
        }

        /// <summary>
        /// Wraps the default <see cref="Mapper{TSource, TTarget}"/>
        /// instance which allows unmapped target members.
        /// </summary>
        internal static class AllSourceMembersMapper<TSource, TTarget>
        {
            // Lazy initialized to avoid a TypeInitializationException in case of a bad mapping,
            private static Mapper<TSource, TTarget> _instance;

            /// <summary>
            /// Default <see cref="Mapper{TSource, TTarget}"/> instance.
            /// *DO NOT* mutate this instance.
            /// </summary>
            internal static Mapper<TSource, TTarget> Instance
            {
                get
                {
                    if (_instance == null) {
                        _instance = CreateMapper();
                    }

                    return _instance;
                }
            }

            /// <summary>
            /// Creates a new mapper instance using appropriate member mapping rules.
            /// </summary>
            private static Mapper<TSource, TTarget> CreateMapper()
            {
                MapperBuilder<TSource, TTarget> config = new MapperBuilder<TSource, TTarget> {
                    AllowUnmappedSourceMembers = false,
                    AllowUnmappedTargetMembers = true
                };

                return config.BuildMapper();
            }
        }

        /// <summary>
        /// Wraps the default <see cref="Mapper{TSource, TTarget}"/>
        /// instance which allows unmapped source members.
        /// </summary>
        internal static class AllTargetMembersMapper<TSource, TTarget>
        {
            // Lazy initialized to avoid a TypeInitializationException in case of a bad mapping,
            private static Mapper<TSource, TTarget> _instance;

            /// <summary>
            /// Default <see cref="Mapper{TSource, TTarget}"/> instance.
            /// *DO NOT* mutate this instance.
            /// </summary>
            internal static Mapper<TSource, TTarget> Instance
            {
                get
                {
                    if (_instance == null) {
                        _instance = CreateMapper();
                    }

                    return _instance;
                }
            }

            /// <summary>
            /// Creates a new mapper instance using appropriate member mapping rules.
            /// </summary>
            private static Mapper<TSource, TTarget> CreateMapper()
            {
                MapperBuilder<TSource, TTarget> config = new MapperBuilder<TSource, TTarget> {
                    AllowUnmappedSourceMembers = true,
                    AllowUnmappedTargetMembers = false
                };

                return config.BuildMapper();
            }
        }
    }
}