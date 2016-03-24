﻿using Kirkin.Reflection;

using Xunit;

namespace Kirkin.Tests.Reflection
{
    public class PropertyInfoExtensionsTests
    {
        [Fact]
        public void IsStaticBenchmark()
        {
            var idProperty = typeof(Dummy).GetProperty("ID");
            var valueProperty = typeof(Dummy).GetProperty("Value");

            for (var i = 0; i < 10000; i++)
            {
                Assert.False(idProperty.IsStatic());
                Assert.True(valueProperty.IsStatic());
            }
        }

        private class Dummy
        {
            public int ID { get; set; }
            public static string Value { get; set; }
        }
    }
}
