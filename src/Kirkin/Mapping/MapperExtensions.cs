namespace Kirkin.Mapping
{
    /// <summary>
    /// Constrained <see cref="IMapper{TSource, TTarget}" /> extensions.
    /// </summary>
    public static class MapperExtensions
    {
        // Not defined on Mapper to avoid introducing a confusing
        // overload for an often used static Map method.

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