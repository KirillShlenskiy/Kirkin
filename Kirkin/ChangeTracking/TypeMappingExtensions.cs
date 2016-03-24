﻿using System;
using System.Collections.Generic;

namespace Kirkin.ChangeTracking
{
    /// <summary>
    /// Common extensions for <see cref="TypeMapping{T}"/>.
    /// </summary>
    public static class TypeMappingExtensions
    {
        /// <summary>
        /// Creates a snapshot of all readable properties
        /// of the target object mapped by this instance.
        /// </summary>
        internal static PropertyValueSnapshot<T> Snapshot<T>(this TypeMapping<T> typeMapping, T target)
        {
            if (target == null) throw new ArgumentNullException("target");

            return new PropertyValueSnapshot<T>(target, typeMapping);
        }

        /// <summary>
        /// Enumerates the values of the target object's
        /// properties encapsulated by this mapping.
        /// </summary>
        public static IEnumerable<PropertyValue> PropertyValues<T>(this TypeMapping<T> typeMapping, T target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            foreach (TypeMapping<T>.PropertyAccessor accessor in typeMapping.PropertyAccessors) {
                yield return new PropertyValue(accessor.Property, accessor.GetValue(target));
            }
        }
    }
}