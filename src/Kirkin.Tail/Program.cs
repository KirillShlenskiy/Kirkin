using System;
using System.Reflection;

using Kirkin.Monitoring;

namespace Kirkin.Tail
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                AssemblyName assemblyInfo = executingAssembly.GetName();

                Console.WriteLine($"Real-time log reader utility v{assemblyInfo.Version}. Usage: tail <filename/path>.");
            }
            else if (args.Length > 1)
            {
                Console.WriteLine("Only one arg is supported (file name/path).");
            }
            else
            {
                string filePath = args[0];
                ReactiveFileMonitor monitor = new ReactiveFileMonitor(filePath);

                monitor.LineRead += Console.WriteLine;

                monitor.MonitorAsync().GetAwaiter().GetResult();
            }
        }
    }
}