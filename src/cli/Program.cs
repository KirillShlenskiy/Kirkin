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
                    CommandArg<string> name = hello.DefineOption("name", shortName: "n");

                    hello.Executed += () => Console.WriteLine($"Hello {name.Value}!");
                });

                ICommand command = parser.Parse(args);

                command.Execute();
            }
            catch (Exception ex)
            {
                ConsoleColor color = Console.ForegroundColor;

                try
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(ex.Message);
                }
                finally
                {
                    Console.ForegroundColor = color;
                }

                return ex.HResult;
            }

            return 0;
        }
    }
}