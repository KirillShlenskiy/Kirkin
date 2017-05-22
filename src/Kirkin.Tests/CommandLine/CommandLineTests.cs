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
            string subscription = null;
            bool validate = false;

            parser.DefineCommand("sync", sync =>
            {
                Func<string> subscriptionOption = sync.DefineOption("subscription", null, args => args.Single());
                Func<bool> validateOption = sync.DefineOption("validate", "v", ParseBoolean);

                return () =>
                {
                    subscription = subscriptionOption();
                    validate = validateOption();
                };
            });

            ICommand command = parser.Parse("sync --subscription main /validate TRUE".Split(' '));

            command.Execute();

            Assert.AreEqual("main", subscription);
            Assert.True(validate);

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