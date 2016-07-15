using System;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Fluent <see cref="MapperBuilder{TSource, TTarget}"/> extensions.
    /// </summary>
    internal static class MapperBuilderExtensions
    {
        /// <summary>
        /// Executes the given configuration action on this
        /// builder instance and returns the mutated instance.
        /// </summary>
        public static MapperBuilder<TSource, TTarget> Configure<TSource, TTarget>(this MapperBuilder<TSource, TTarget> builder,
                                                                                  Action<MapperBuilder<TSource, TTarget>> configureAction)
        {
            configureAction(builder);

            return builder;
        }
    }
}