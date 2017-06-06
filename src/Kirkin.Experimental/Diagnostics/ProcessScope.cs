using System;
using System.Diagnostics;
using System.Threading;

namespace Kirkin.Diagnostics
{
    public sealed class ProcessScope : IDisposable
    {
        private readonly EventHandler CurrentDomainExitHandler;
        private Process _process;

        public Process Process
        {
            get
            {
                return _process;
            }
        }

        public ProcessScope(Process process)
        {
            _process = process;

            CurrentDomainExitHandler = delegate {
                KillChildProcess();
            };

            AppDomain.CurrentDomain.ProcessExit += CurrentDomainExitHandler;
        }

        private void KillChildProcess()
        {
            Process process = Interlocked.Exchange(ref _process, null);

            if (process != null && !process.HasExited) {
                process.Kill();
            }
        }

        public void Dispose()
        {
            KillChildProcess();

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomainExitHandler;
        }
    }
}