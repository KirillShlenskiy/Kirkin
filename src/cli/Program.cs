using System;

using Kirkin.CommandLine;

namespace cli
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                CommandLineParser parser = new CommandLineParser();

                parser.DefineCommand("hello", hello =>
                {
                    hello.DefineOption("name", shortName: "n");

                    hello.Executed += (s, e) => Console.WriteLine($"Hello {(string)e.Args["name"]}!");
                });

                parser.DefineCommand("goodbye", goodbye =>
                {
                    goodbye.DefineParameter("name");

                    goodbye.Executed += (s, e) => Console.WriteLine($"Goodbye {(string)e.Args["name"]}!");
                });

                ICommand command = parser.Parse(args);

                command.Execute();
            }
            catch (Exception ex)
            {
                using (ConsoleFormatter.ForegroundColorScope(ConsoleColor.Red)) {
                    Console.Error.WriteLine(ex.Message);
                }

                return ex.HResult;
            }

            return 0;
        }
    }
}