using System;

using Kirkin.Reflection;

using NUnit.Framework;

namespace Kirkin.Tests.Reflection
{
    public class MethodTests
    {
        //[Fact(Skip = "TODO")]
        public void CreateDelegateFromGenericExpression()
        {
            Func<string, string> func = Method
                .StaticMethod(() => ContainerType.Process(123))
                .CreateDelegate<Func<string, string>>();

            Assert.AreEqual("zzz", func("zzz"));
        }

        static class ContainerType
        {
            public static T Process<T>(T value)
            {
                return value;
            }
        }
    }
}