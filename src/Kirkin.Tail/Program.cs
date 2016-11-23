using System;
using System.IO;
using System.Reflection;

using Kirkin.Monitoring;

namespace Kirkin.Tail
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Assembly executingAssembly = Assembly.GetExecutingAssembly();
                    AssemblyName assemblyInfo = executingAssembly.GetName();

                    Console.WriteLine($"Real-time log reader utility v{assemblyInfo.Version}. Usage: tail <filename/path>.");

                    return 0;
                }
                else if (args.Length > 1)
                {
                    Console.WriteLine("Only one arg is supported (file name/path).");

                    return -1;
                }
                else
                {
                    string filePath = args[0];
                    FileInfo file = new FileInfo(filePath);

                    if (!file.Exists)
                    {
                        Console.WriteLine($"File '{file.FullName}' does not exist.");

                        return -1;
                    }

                    ReactiveFileMonitor monitor = new ReactiveFileMonitor(file.FullName);

                    monitor.LineRead += Console.WriteLine;

                    monitor.MonitorAsync().GetAwaiter().GetResult();

                    return 0;
                }
            }
            catch (Exception ex)
            {
                ConsoleColor defaultColor = Console.ForegroundColor;

                try
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    Console.ForegroundColor = defaultColor;
                }

                return -1;
            }
        }
    }
}