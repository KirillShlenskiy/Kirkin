using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public class ConsoleRunnerTests
    {
        [Test]
        public async Task Run()
        {
            ConsoleRunner app = new ConsoleRunner("replmon", "sync extra");

            app.Output += Console.WriteLine;

            CancellationTokenSource cts = new CancellationTokenSource();

            cts.CancelAfter(5000);

            await app.RunAsync(cts.Token);
        }
    }
}