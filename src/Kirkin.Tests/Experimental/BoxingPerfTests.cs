using System;

using NUnit.Framework;

namespace Kirkin.Tests.Experimental
{
    public class BoxingPerfTests
    {
        struct MyDisposableStruct : IDisposable
        {
            public void Dispose()
            {
            }
        }

        sealed class MyDisposableClass : IDisposable
        {
            public void Dispose()
            {
            }
        }

        [Test]
        public void StructNoBoxing()
        {
            var disp = new MyDisposableStruct();

            for (int i = 0; i < 100000000; i++)
            {
                using (disp)
                {
                }
            }
        }

        [Test]
        public void StructBoxing()
        {
            var disp = (IDisposable)new MyDisposableStruct();

            for (int i = 0; i < 100000000; i++)
            {
                using (disp)
                {
                }
            }
        }

        [Test]
        public void ClassDirect()
        {
            var disp = new MyDisposableClass();

            for (int i = 0; i < 100000000; i++)
            {
                using (disp)
                {
                }
            }
        }

        [Test]
        public void ClassViaInterface()
        {
            var disp = (IDisposable)new MyDisposableClass();

            for (int i = 0; i < 100000000; i++)
            {
                using (disp)
                {
                }
            }
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}