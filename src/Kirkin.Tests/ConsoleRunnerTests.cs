using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;
using System.Windows.Forms;

namespace Kirkin.Tests
{
    [Ignore("External dependencies")]
    public class ConsoleRunnerTests
    {
        [Test]
        public async Task AsyncRun()
        {
            SynchronizationContext.SetSynchronizationContext(null);

            ConsoleRunner app = new ConsoleRunner("replmon", "sync extra");

            app.Output += Console.WriteLine;

            CancellationTokenSource cts = new CancellationTokenSource();

            cts.CancelAfter(500);

            try
            {
                await app.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        [Test]
        public void SyncRunNoDeadlock()
        {
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

            try
            {
                ConsoleRunner app = new ConsoleRunner("replmon", "sync extra");

                app.Output += Console.WriteLine;

                app.RunAsync().GetAwaiter().GetResult();
            }
            finally
            {
                WindowsFormsSynchronizationContext.Uninstall();
            }
        }

        // Proof of concept.
        //[Test]
        public void Deadlock()
        {
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

            try
            {
                Func<Task> f = async () =>
                {
                    await Task.Delay(100);
                    await Task.Delay(100);
                };

                f().GetAwaiter().GetResult();
            }
            finally
            {
                WindowsFormsSynchronizationContext.Uninstall();
            }
        }
    }
}