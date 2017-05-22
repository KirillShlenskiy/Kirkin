using System;
using System.Linq;

using Kirkin.CommandLine;

using NUnit.Framework;

namespace Kirkin.Tests.CommandLine
{
    public class CommandLineTests
    {
        [Test]
        public void BasicCommandLineParsing()
        {
            CommandLineParser parser = new CommandLineParser();
            Func<string[], bool> ParseBoolean = args => args.Length == 0 || Convert.ToBoolean(args.Single());

            parser.DefineCommand("sync", sync =>
            {
                Func<bool> validate = sync.DefineOption("validate", "v", ParseBoolean);
                Func<string> subscription = sync.DefineOption("subscription", null, args => args.Single());

                return () => {
                    Console.WriteLine($"sync {subscription()} validate:{validate()}.");
                };
            });

            ICommand command = parser.Parse("sync --subscription main /validate TRUE".Split(' '));

            command.Execute();

            //Command sync = new Command("sync");
            //bool verify = false;

            //sync.DefineOption("v", "verify", ref verify);
            //sync.DefineParameter()

            //sync.Invoked += () =>
            //{
            //    if (verify)
            //    {

            //    }
            //    else
            //    {

            //    }
            //};
        }
    }
}