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
                    hello.DefineParameter("name");
                    hello.DefineSwitch("sir", shortName: "s");
                    hello.DefineOption("surname");

                    hello.Executed += (s, e) =>
                    {
                        string name = (string)e.Args["name"];
                        bool sir = (bool)e.Args["sir"];
                        string surname = (string)e.Args["surname"];

                        Console.WriteLine($"Hello {(sir ? "sir " : "")}{name}{(surname == null ? "" : " " + surname)}!");
                    };
                });

                parser.DefineCommand("goodbye", goodbye =>
                {
                    goodbye.DefineParameter("name");
                    goodbye.DefineSwitch("sir", shortName: "s");
                    goodbye.DefineOption("surname");

                    goodbye.Executed += (s, e) =>
                    {
                        string name = (string)e.Args["name"];
                        bool sir = (bool)e.Args["sir"];
                        string surname = (string)e.Args["surname"];

                        Console.WriteLine($"Goodbye {(sir ? "sir " : "")}{name}{(surname == null ? "" : " " + surname)}");
                    };
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