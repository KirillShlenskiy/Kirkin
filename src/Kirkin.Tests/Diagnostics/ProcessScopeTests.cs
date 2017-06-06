using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using Kirkin.Diagnostics;

using NUnit.Framework;

namespace Kirkin.Tests.Diagnostics
{
    public class ProcessScopeTests
    {
        [Test]
        public void AppDomainTest()
        {
            IsolationContext isolated = new IsolationContext();
            IsolatedTest test = isolated.CreateInstance<IsolatedTest>();
            int processID = test.CreateCmdProcess();
            Process cmd = Process.GetProcessById(processID);

            Thread.Sleep(100);
            Assert.False(cmd.HasExited);

            isolated.Dispose();

            Thread.Sleep(100);
            Assert.True(cmd.HasExited);
        }

        public class IsolatedTest : MarshalByRefObject
        {
            public int CreateCmdProcess()
            {
                Process cmd = Process.Start(new ProcessStartInfo("cmd") {
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                });

                ProcessScope scope = new ProcessScope(cmd);

                return cmd.Id;
            }
        }
    }
}