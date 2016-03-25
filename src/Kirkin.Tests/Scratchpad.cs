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
    }
}