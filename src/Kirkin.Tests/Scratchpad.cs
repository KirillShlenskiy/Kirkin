using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Logging;
using Kirkin.Reflection;

using Xunit;
using Xunit.Abstractions;

namespace Kirkin.Tests
{
    public class Scratchpad
    {
        private readonly Logger Output;

        public Scratchpad(ITestOutputHelper output)
        {
            Output = Logger
                .Create(output.WriteLine)
                .WithFormatters(EntryFormatter.TimestampNonEmptyEntries());
        }


        [Fact]
        public void GetRightFactoryWithActivator()
        {
            PropertyInfo property = typeof(string).GetProperty("Length");

            for (int i = 0; i < 200000; i++)
            {
                IPropertyAccessorFactory factory = (IPropertyAccessorFactory)Activator.CreateInstance(
                    typeof(PropertyAccessorFactory<,>).MakeGenericType(property.DeclaringType, property.PropertyType)
                );

                IPropertyAccessor accessor = factory.Create(property);
            }
        }

        //[Fact]
        //public void GetRightFactoryWithConstructorInfo()
        //{
        //    PropertyInfo property = typeof(string).GetProperty("Length");

        //    for (int i = 0; i < 200000; i++)
        //    {
        //        IPropertyAccessor accessor = (IPropertyAccessor)typeof(PropertyAccessor<,>)
        //            .MakeGenericType(property.DeclaringType, property.PropertyType)
        //            .GetConstructor(new[] { typeof(PropertyInfo) })
        //            .Invoke(new[] { property });

        //        Assert.NotNull(accessor);
        //    }
        //}

        interface IPropertyAccessorFactory
        {
            IPropertyAccessor Create(PropertyInfo property);
        }

        sealed class PropertyAccessorFactory<TTarget, TProperty>
            : IPropertyAccessorFactory
        {
            public IPropertyAccessor Create(PropertyInfo property)
            {
                return new PropertyAccessor<TTarget, TProperty>(property);
            }
        }
    }
}