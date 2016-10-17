using System;

using Kirkin.Mapping.Fluent;
using Kirkin.Reflection;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Constrained <see cref="IMapper{TSource, TTarget}" /> extensions.
    /// </summary>
    public static class Extensions
    {
        #region Mapper extensions

        // Not defined on Mapper to avoid introducing a confusing
        // overload for an often used static Map method.

        /// <summary>
        /// Creates a new target instance, executes mapping from source
        /// to target and returns the newly created target instance.
        /// </summary>
        public static TTarget Map<TSource, TTarget>(this Mapper<TSource, TTarget> mapper, TSource source)
            where TTarget : new()
        {
            return mapper.Map(source, new TTarget());
        }

        #endregion

        #region Builder and fluent config extensions

        /// <summary>
        /// Executes the given configuration action on this
        /// builder instance and returns the mutated instance.
        /// </summary>
        internal static MapperBuilder<TSource, TTarget> Configure<TSource, TTarget>(this MapperBuilder<TSource, TTarget> builder,
                                                                                    Action<MapperBuilder<TSource, TTarget>> configureAction)
        {
            configureAction(builder);

            return builder;
        }

        /// <summary>
        /// Produces an intermediate factory object that can create <see cref="MapperBuilder{TSource, TTarget}"/>
        /// instances mapping from the specified properties of the given type to various target types.
        /// </summary>
        public static PartiallyConfiguredMapperBuilder<TSource> FromPropertyList<TSource>(this MapperBuilderFactory factory, PropertyList<TSource> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            Member<TSource>[] sourceMembers = PropertyMember.PropertyListMembers(propertyList);

            return factory.FromMembers(sourceMembers);
        }

        /// <summary>
        /// Creates a <see cref="MapperBuilder{TSource, TTarget}"/> which defines a
        /// mapping from source to the specified members of the given target type. 
        /// </summary>
        public static MapperBuilder<TSource, TTarget> ToPropertyList<TSource, TTarget>(this PartiallyConfiguredMapperBuilder<TSource> factory, PropertyList<TTarget> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            Member<TTarget>[] targetMembers = PropertyMember.PropertyListMembers(propertyList);

            return factory.ToMembers(targetMembers);
        }

        #endregion
    }
}