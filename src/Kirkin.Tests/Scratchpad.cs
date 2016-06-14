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

        public void Inj()
        {
            InjectorBuilder<IDummy> builder = new InjectorBuilder<IDummy>();

            builder.SubstituteFunc<int>(
                d => d.GetValue, func => () => func() + 1
            );

            builder.SubstituteFunc<int, int>(
                d => d.AddThree(0), func => i => func(i)
            );

            IInjector<IDummy> injector = builder.BuildInjector();
            IDummy dummy = injector.Inject(new Dummy());

            Assert.Equal(42, dummy.GetValue());
        }

        interface IDummy
        {
            int GetValue();
        }

        sealed class Dummy : IDummy
        {
            public int GetValue()
            {
                return 42;
            }

            public int AddThree(int i)
            {
                return i + 3;
            }
        }
    }
}