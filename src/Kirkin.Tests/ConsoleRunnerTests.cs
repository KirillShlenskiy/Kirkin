using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public class ConsoleRunnerTests
    {
        [Test]
        public void RunSimple()
        {
            ConsoleRunner app = new ConsoleRunner("cmd", args: @"/C dir C:\");
            List<string> messages = new List<string>();

            app.Output += messages.Add;

            app.Run();

            foreach (string message in messages) {
                Console.WriteLine(message);
            }
        }

        [Test]
        public void RunWithInput()
        {
            ConsoleRunner app = new ConsoleRunner("cmd");
            List<string> messages = new List<string>();

            app.Output += messages.Add;

            Task runTask = app.RunAsync();

            app.Process.StandardInput.WriteLine(@"dir C:\");

            Thread.Sleep(100);
            Assert.False(runTask.IsCompleted);

            app.Process.StandardInput.WriteLine("exit");

            Thread.Sleep(100);
            Assert.True(runTask.IsCompleted);

            foreach (string message in messages) {
                Console.WriteLine(message);
            }
        }

        [Test]
        public async Task AsyncRun()
        {
            ConsoleRunner app = new ConsoleRunner("cmd");

            app.Output += Console.WriteLine;

            CancellationTokenSource cts = new CancellationTokenSource();

            cts.CancelAfter(500);

            bool canceled = false;

            try
            {
                await app.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                canceled = true;
            }

            Assert.True(canceled, "Expecting the operation to have been canceled.");
        }

        [Test]
        [Ignore("External dependencies")]
        public void SyncRunNoDeadlock()
        {
            RunWithSyncContext(syncContext =>
            {
                Action<string> output = s =>
                {
                    Console.WriteLine(s);
                    //syncContext.Send(_ => Console.WriteLine(s), null);
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