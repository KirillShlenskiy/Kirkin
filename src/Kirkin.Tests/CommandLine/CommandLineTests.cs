﻿using System;

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

            parser.DefineCommand("sync", sync =>
            {
                sync.DefineOption("subscription", "s");
                sync.DefineSwitch("validate", "v");

                sync.Executed += (s, e) =>
                {
                    subscription = (string)e.Args["subscription"];
                    validate = (bool)e.Args["validate"];
                };
            });

            Assert.Throws<InvalidOperationException>(() => parser.Parse("uuu"));

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

        [Test]
        public void Parameters()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("sync", sync =>
            {
                sync.DefineParameter("subscription");
                sync.DefineSwitch("validate", shortName: "v");
            });

            ICommand command = parser.Parse("sync extra --validate".Split(' '));

            Assert.AreEqual("extra", (string)command.Arguments["subscription"]);
            Assert.True((bool)command.Arguments["validate"]);
        }

        [Test]
        public void DefaultValues()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("sync", sync =>
            {
                sync.DefineParameter("subscription");
                sync.DefineSwitch("validate", shortName: "v");
                sync.DefineOption("log", shortName: "l");
            });

            {
                ICommand command = parser.Parse("sync --validate --log zzz.txt".Split(' '));

                Assert.Null(command.Arguments["subscription"]);
            }

            {
                ICommand command = parser.Parse("sync main --log zzz.txt".Split(' '));

                Assert.False((bool)command.Arguments["validate"]);
            }

            {
                ICommand command = parser.Parse("sync main --validate".Split(' '));

                Assert.Null(command.Arguments["log"]);
            }
        }

        [Test]
        public void OptionValueVariants()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("sync", sync =>
            {
                sync.DefineParameter("subscription");
                sync.DefineOption("log", shortName: "l");
            });

            {
                ICommand command = parser.Parse("sync --log zzz.txt".Split(' '));

                Assert.AreEqual("zzz.txt", command.Arguments["log"]);
            }

            {
                ICommand command = parser.Parse("sync --log=zzz.txt".Split(' '));

                Assert.AreEqual("zzz.txt", command.Arguments["log"]);
            }

            {
                ICommand command = parser.Parse("sync --log:zzz.txt".Split(' '));

                Assert.AreEqual("zzz.txt", command.Arguments["log"]);
            }
        }
    }
}