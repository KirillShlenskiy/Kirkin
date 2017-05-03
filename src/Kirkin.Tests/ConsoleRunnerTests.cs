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
            RunWithSyncContext(syncContext =>
            {
                Action<string> output = s => {
                    syncContext.Send(_ => Console.WriteLine(s), null);
                };

                ConsoleRunner app = new ConsoleRunner("replmon", "sync extra");

                app.Output += output;

                app.Run();
            });
        }

        // Proof of concept.
        //[Test]
        public void Deadlock()
        {
            RunWithSyncContext(_ =>
            {
                Func<Task> f = async () =>
                {
                    await Task.Delay(100);
                    await Task.Delay(100);
                };

                f().GetAwaiter().GetResult();
            });
        }

        static void RunWithSyncContext(Action<SynchronizationContext> action)
        {
            WindowsFormsSynchronizationContext syncContext = new WindowsFormsSynchronizationContext();

            SynchronizationContext.SetSynchronizationContext(syncContext);

            try
            {
                action(syncContext);
            }
            finally
            {
                WindowsFormsSynchronizationContext.Uninstall();
            }
        }
    }
}