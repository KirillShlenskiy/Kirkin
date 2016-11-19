using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Kirkin.Tests.Experimental
{
    public class ImmutableArrayFactoryTests
    {
        [Fact]
        public void BasicApi()
        {
            int[] integers = { 1, 2, 3 };
            ImmutableArray<int> immutable = ImmutableArrayFactory.WrapWithReflection(integers);

            Assert.Equal(integers, immutable);
            Assert.False(immutable.IsDefault);

            ImmutableArray<int> def = new ImmutableArray<int>();

            Assert.True(def.IsDefault);
            Assert.Equal(0, ImmutableArray.Create<int>().Length);
        }

        [Fact]
        public void WrapWithReflectionPerf()
        {
            int[] integers = { 1, 2, 3 };
            ImmutableArray<int> immutable;

            for (int i = 0; i < 250000; i++) {
                immutable = ImmutableArrayFactory.WrapWithReflection(integers);
            }
        }

        public static class ImmutableArrayFactory
        {
            /// <summary>
            /// Creates an <see cref="ImmutableArray{T}"/> wrapper around the given array.
            /// The resulting array won't be truly immutable, but the consumers will think it is
            /// and the performance will be better than copy can provide.
            /// </summary>
            public static ImmutableArray<T> WrapWithReflection<T>(T[] array)
            {
                ImmutableArray<T> immutable = new ImmutableArray<T>();
                FieldInfo arrayField = Field<ImmutableArray<T>>.Value;
                TypedReference typeRef = __makeref(immutable);

                arrayField.SetValueDirect(typeRef, array);

                return immutable;
            }

            //static class FieldDelegate<TType, TField>
            //{
            //    private static Action<TType, TField> _setter;

            //    internal static Action<TType, TField> Setter
            //    {
            //        get
            //        {

            //        }
            //    }
            //}

            static class Field<T>
            {
                private static FieldInfo _value;

                internal static FieldInfo Value
                {
                    get
                    {
                        if (_value == null) {
                            _value = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)[0];
                        }

                        return _value;
                    }
                }
            }
        }
    }
}