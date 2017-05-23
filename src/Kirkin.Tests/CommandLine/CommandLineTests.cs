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

            parser.DefineCommand("zzz", zzz =>
            {
                zzz.DefineOption("fizz");
                zzz.DefineOption("buzz");
                zzz.DefineSwitch("holy-moly", shortName: "hm");
            });

            string subscription = null;
            bool validate = false;
            CommandArg<string> subscriptionArg = null;

            parser.DefineCommand("sync", sync =>
            {
                subscriptionArg = sync.DefineOption("subscription", "s");
                CommandArg<bool> validateArg = sync.DefineSwitch("validate", "v");

                sync.Executed += () =>
                {
                    subscription = subscriptionArg.Value;
                    validate = validateArg.Value;
                };
            });

            Assert.Throws<InvalidOperationException>(() => parser.Parse("uuu"));
            Assert.Throws<InvalidOperationException>(() => { var _ = subscriptionArg.Value; });

            {
                ICommand command = parser.Parse("sync --subscription main /VALIDATE TRUE".Split(' '));

                Assert.Null(subscription);
                Assert.False(validate);

                command.Execute();

                Assert.AreEqual("main", subscription);
                Assert.True(validate);
            }

            {
                ICommand command = parser.Parse("sync --subscription extra".Split(' '));

                Assert.AreEqual("main", subscription);
                Assert.True(validate);

                command.Execute();

                Assert.AreEqual("extra", subscription);
                Assert.False(validate);
            }

            parser.Parse("sync --validate".Split(' ')).Execute();

            Assert.Null(subscription);
            Assert.True(validate);

            parser.Parse("sync --validate false".Split(' ')).Execute();

            Assert.Null(subscription);
            Assert.False(validate);

            parser.Parse("sync -s zzz -v".Split(' ')).Execute();

            Assert.AreEqual("zzz", subscription);
            Assert.True(validate);

            // Alternative arg syntax.
            {
                ICommand command = parser.Parse("zzz --fizz 1 /buzz ultra --holy-moly".Split(' '));

                Assert.AreEqual("1", (string)command.Arguments["fizz"]);
                Assert.AreEqual("ultra", (string)command.Arguments["buzz"]);
                Assert.True((bool)command.Arguments["holy-moly"]);
            }
        }
    }
}