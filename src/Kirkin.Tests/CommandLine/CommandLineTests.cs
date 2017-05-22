using System;

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
            Func<string, bool> ParseBoolean = value => string.IsNullOrEmpty(value) || Convert.ToBoolean(value);
            string subscription = null;
            bool validate = false;

            parser.DefineCommand("sync", sync =>
            {
                Func<string> subscriptionOption = sync.DefineOption("subscription", null);
                Func<bool> validateOption = sync.DefineOption("validate", "v", ParseBoolean);

                return () =>
                {
                    subscription = subscriptionOption();
                    validate = validateOption();
                };
            });

            ICommand command = parser.Parse("sync --subscription main /VALIDATE TRUE".Split(' '));

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