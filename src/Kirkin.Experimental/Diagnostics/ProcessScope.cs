using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (!Process.HasExited)
            {
                Process.Kill();
            }
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomainExitHandler;
        }
    }
}