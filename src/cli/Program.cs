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
                    hello.Help = "Greets the user";

                    hello.DefineParameter("name", help: "Name of the person to be greeted.");
                    hello.DefineSwitch("sir", shortName: "s", help: "Specify if you want the name to be prefixed with 'sir'.");
                    hello.DefineOption("surname", help: "Optional surname of the person to be greeted.");

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
                    goodbye.Help = "Farewells the user";

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