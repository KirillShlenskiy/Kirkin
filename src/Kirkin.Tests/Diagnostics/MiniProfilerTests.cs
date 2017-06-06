using System;
using System.Threading;

using Kirkin.Diagnostics;

using NUnit.Framework;

namespace Kirkin.Tests.Diagnostics
{
    public class MiniProfilerTests
    {
        [Test]
        public void BasicApi()
        {
            MiniProfiler profiler = new MiniProfiler();

            profiler.Time("operation 1", () => Thread.Sleep(10));
            profiler.Time("operation 1", () => Thread.Sleep(10));
            profiler.Time("operation 1", () => Thread.Sleep(10));

            profiler.Time("operation 2", () => Thread.Sleep(50));

            using (profiler.Time("operation 2")) Thread.Sleep(30);
            using (profiler.Time("operation 2")) Thread.Sleep(80);

            Console.WriteLine(profiler.Operations[0].ToString());
            Console.WriteLine(profiler.Operations[1].ToString());
        }
    }
}