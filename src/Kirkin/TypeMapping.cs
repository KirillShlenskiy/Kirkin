using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Kirkin.Collections.Generic;
using Kirkin.Linq.Expressions;

#if !__MOBILE__
using Kirkin.Reflection;
#endif

namespace Kirkin
{
    /// <summary>
    /// Static <see cref="TypeMapping{T}.Default" /> proxy methods.
    /// Supports basic cloning and mapping operations. If you need to
    /// support advanced mapping scenarios (such as mapping via an interface
    /// or mapping to a derived type, use <see cref="TypeMapping{T}"/> instead.
    /// </summary>
    /// <remarks>
    /// The proxy methods are more restrictive than their counterparts defined on <see cref="TypeMapping{T}" />.
    /// The reason for having them in the first place is scenarios where we're mapping instances of the same type.
    /// In those cases it is often better to let type inference do its job than specify T manually, to prevent
    /// possible data loss if parameter type changes to a more derived type, but T is not updated at the same time.
    /// </remarks>
    public static class TypeMapping
    {
        /// <summary>
        /// TypeMapping{T}.Default.Clone proxy method.
        /// Creates a shallow clone of the given object.
        /// </summary>
        public static T Clone<T>(T original)
            where T : new()
        {
            return TypeMapping<T>.Default.Clone(original);
        }

        /// <summary>
        /// TypeMapping{T}.Default.Map proxy method.
        /// Reconciles the differences where necessary,
        /// and returns the target instance.
        /// </summary>
        public static T Map<T>(T source, T target)
        {
            return TypeMapping<T>.Default.Map(source, target);
        }
    }

    /// <summary>
    /// Reflection-based property mapper.
    /// </summary>
    public sealed class TypeMapping<T>
    {
        /// <summary>
        /// Default mapping instance for type T. Maps all properties
        /// which have an accessible getter for read (compare/hash)
        /// operations, and all properties which have accessible
        /// getters and setters for write (copy/clone) operations.
        /// </summary>
        public static TypeMapping<T> Default { get; }

        /// <summary>
        /// Type initialiser.
        /// </summary>
        static TypeMapping()
        {
            Default = CreateDefault();
        }

        /// <summary>
        /// Creates an instance of <see cref="TypeMapping{T}"/> with default parameters.
        /// </summary>
        private static TypeMapping<T> CreateDefault()
        {
            Array<PropertyAccessor>.Builder accessors = new Array<PropertyAccessor>.Builder();

            foreach (PropertyInfo prop in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.CanRead) {
                    accessors.Add(new PropertyAccessor(prop));
                }
            }

            return new TypeMapping<T>(accessors.ToVector());
        }

        /// <summary>
        /// Accessors for readable properties mapped by this instance.
        /// </summary>
        internal readonly Vector<PropertyAccessor> PropertyAccessors;

        /// <summary>
        /// Gets the properties which are mapped by this instance.
        /// </summary>
        public IEnumerable<PropertyInfo> MappedProperties
        {
            get
            {
                foreach (PropertyAccessor accessor in PropertyAccessors) {
                    yield return accessor.Property;
                }
            }
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        private TypeMapping(Vector<PropertyAccessor> propertyAccessors)
        {
            PropertyAccessors = propertyAccessors;
        }

        /// <summary>
        /// Returns a new instance of TypeMapping
        /// with the given property excluded from
        /// the collection of mapped properties.
        /// </summary>
        public TypeMapping<T> Without<TProperty>(Expression<Func<T, TProperty>> propertyExpr)
        {
            if (propertyExpr == null) throw new ArgumentNullException("propertyExpr");

            PropertyInfo excludedProperty = ExpressionUtil.Property(propertyExpr);
            Array<PropertyAccessor>.Builder accessors = new Array<PropertyAccessor>.Builder(PropertyAccessors.Length - 1);

            foreach (PropertyAccessor accessor in PropertyAccessors)
            {
                if (!TokenEquals(accessor.Property, excludedProperty)) {
                    accessors.Add(accessor);
                }
            }

            return new TypeMapping<T>(accessors.ToVector());
        }

        /// <summary>
        /// Creates a shallow clone of the given object.
        /// </summary>
        public TTarget Clone<TTarget>(TTarget original)
            where TTarget : T, new()
        {
            if (original == null) throw new ArgumentNullException("original");

            return Map(original, new TTarget());
        }

        /// <summary>
        /// Reconciles the differences where necessary,
        /// and returns the target instance.
        /// </summary>
        public TTarget Map<TTarget>(T source, TTarget target)
            where TTarget : T // Target can be derived from source.
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");

            foreach (PropertyAccessor accessor in PropertyAccessors)
            {
                if (accessor.Property.CanWrite)
                {
                    object newValue = accessor.GetValue(source);
                    object oldValue = accessor.GetValue(target);

                    if (!Equals(newValue, oldValue)) {
                        accessor.SetValue(target, newValue);
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// Reconciles the differences where necessary,
        /// and returns the target instance and number
        /// of changes applied.
        /// </summary>
        public TTarget Map<TTarget>(T source, TTarget target, out int changeCount)
            where TTarget : T // Target can be derived from source.
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");

            changeCount = 0;

            foreach (PropertyAccessor accessor in PropertyAccessors)
            {
                if (accessor.Property.CanWrite)
                {
                    object newValue = accessor.GetValue(source);
                    object oldValue = accessor.GetValue(target);

                    if (!Equals(newValue, oldValue))
                    {
                        accessor.SetValue(target, newValue);
                        changeCount++;
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// Returns a string describing the object
        /// which includes values of all mapped properties.
        /// </summary>
        public string ToString(T obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            // Comma-separated list of
            // property names and their values.
            StringBuilder sb = new StringBuilder();
            Type actualType = obj.GetType();

            // TypeName redefined here as opposed to TypeUtil in order to fix VS2015/ Xamarin
            // compiler bug where TypeUtil is not visible even despite relevant compiler switches.
            // Will need to revert to using TypeUtil.TypeName whenever the fix is rolled out.
            sb.Append(TypeName(actualType));
            sb.Append(" { ");

            bool needComma = false;

            foreach (PropertyAccessor accessor in PropertyAccessors)
            {
                if (needComma) {
                    sb.Append(", ");
                }

                sb.Append(accessor.Property.Name);
                sb.Append(" = ");
                sb.Append(accessor.GetValue(obj));

                needComma = true;
            }

            sb.Append(" }");

            return sb.ToString();
        }

        /// <summary>
        /// Performs advanced equality testing for scenarios
        /// where Equals does not suffice (i.e. PropertyInfo
        /// instances which were obtained from base and derived
        /// types respectively).
        /// </summary>
        private static bool TokenEquals(PropertyInfo propertyInfo, PropertyInfo other)
        {
            return propertyInfo.Module == other.Module
                && propertyInfo.MetadataToken == other.MetadataToken;
        }

        /// <summary>
        /// Gets a meaningful description of the type, including any generic type arguments.
        /// </summary>
        private static string TypeName(Type type)
        {
            if (!type.IsGenericType) {
                return type.Name;
            }

            Type[] genericArgTypes = type.GetGenericArguments();
            StringBuilder sb = new StringBuilder();

            sb.Append(type.Name, 0, type.Name.IndexOf('`'));
            sb.Append('<');

            for (int i = 0; i < genericArgTypes.Length; i++)
            {
                if (i != 0)
                {
                    sb.Append(',');
                    sb.Append(' ');
                }

                sb.Append(TypeName(genericArgTypes[i]));
            }

            sb.Append('>');

            return sb.ToString();
        }

        #region PropertyAccessor definition

#if !__MOBILE__
        internal struct PropertyAccessor
        {
            private readonly IFastProperty __property;

            public PropertyInfo Property
            {
                get { return __property.Property; }
            }

            public PropertyAccessor(PropertyInfo property)
            {
                __property = TypeUtil<T>.Property(property);
            }

            public object GetValue(object obj)
            {
                return __property.GetValue(obj);
            }

            public void SetValue(object obj, object value)
            {
                __property.SetValue(obj, value);
            }
        }
#else
        internal struct PropertyAccessor
        {
            private readonly PropertyInfo __property;

            public PropertyInfo Property
            {
                get { return __property; }
            }

            public PropertyAccessor(PropertyInfo property)
            {
                __property = property;
            }

            public object GetValue(object obj)
            {
                return __property.GetValue(obj, null);
            }

            public void SetValue(object obj, object value)
            {
                __property.SetValue(obj, value, null);
            }
        }
#endif
        #endregion
    }
}