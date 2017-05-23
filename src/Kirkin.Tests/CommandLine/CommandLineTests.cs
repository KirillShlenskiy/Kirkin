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
            string subscription = null;
            bool validate = false;
            Arg<string> subscriptionArg = null;

            parser.DefineCommand("sync", sync =>
            {
                subscriptionArg = sync.DefineOption("subscription");
                Arg<bool> validateArg = sync.DefineSwitch("validate", "v");

                sync.Executed += () =>
                {
                    subscription = subscriptionArg.Value;
                    validate = validateArg.Value;
                };
            });

            Assert.Throws<InvalidOperationException>(() => { var _ = subscriptionArg.Value; });

            ICommand command = parser.Parse("sync --subscription main /VALIDATE TRUE".Split(' '));

            Assert.Null(subscription);
            Assert.False(validate);

            command.Execute();

            Assert.AreEqual("main", subscription);
            Assert.True(validate);

            command = parser.Parse("sync --subscription extra".Split(' '));

            Assert.AreEqual("main", subscription);
            Assert.True(validate);

            command.Execute();

            Assert.AreEqual("extra", subscription);
            Assert.False(validate);

            parser.Parse("sync --validate".Split(' ')).Execute();

            Assert.Null(subscription);
            Assert.True(validate);
        }
    }
}