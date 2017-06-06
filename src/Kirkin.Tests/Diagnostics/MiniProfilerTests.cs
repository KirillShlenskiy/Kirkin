using System;
using System.Linq;
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

            profiler.BeginTime("operation 2");
            Thread.Sleep(40);
            profiler.EndTime("operation 2");

            profiler.BeginTime("operation 2");
            Thread.Sleep(60);
            profiler.EndTime("operation 2");

            using (profiler.Time("operation 2")) Thread.Sleep(80);

            MiniProfiler.Operation[] operations = profiler.Operations.ToArray();

            Console.WriteLine(operations[0].ToString());
            Console.WriteLine(operations[1].ToString());
        }
    }
}