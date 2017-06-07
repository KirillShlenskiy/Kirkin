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
            CommandLineParser parser = new CommandLineParser {
                StringEqualityComparer = StringComparer.OrdinalIgnoreCase
            };

            parser.DefineCommand("zzz", "Prints Holy Moly", zzz =>
            {
                zzz.DefineOption("fizz");
                zzz.DefineOption("buzz");
                zzz.DefineSwitch("holy-moly", shortName: "hm");
            });

            string subscription = null;
            bool validate = false;

            parser.DefineCommand("sync", "Performs database synchronisation", sync =>
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

            parser.DefineCommand("sync", "Performs database synchronisation", sync =>
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

            parser.DefineCommand("sync", "Performs database synchronisation", sync =>
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

            parser.DefineCommand("sync", "Performs database synchronisation", sync =>
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

        [Test]
        public void ToStringTest()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("sync", "Performs database synchronisation", sync =>
            {
                sync.DefineParameter("subscription");
                sync.DefineSwitch("validate", shortName: "v");
                sync.DefineOption("log", shortName: "l");
            });

            Assert.AreEqual("sync <subscription> [-v|--validate] [-l|--log <arg>]", parser.CommandDefinitions[0].ToString());
        }

        [Test]
        public void ParameterAndOptionList()
        {
            CommandLineParser parser = new CommandLineParser();
            string parameterValue = null;
            bool? switchValue = null;
            string optionValue = null;

            parser.DefineCommand("hello", hello =>
            {
                hello.DefineParameterList("names");
                hello.DefineSwitch("switch");
                hello.DefineOptionList("colors");

                hello.Executed += (s, e) =>
                {
                    parameterValue = string.Join(", ", (string[])e.Args["names"]);
                    switchValue = (bool)e.Args["switch"];
                    optionValue = string.Join(", ", (string[])e.Args["colors"]);
                };
            });

            ICommand command = parser.Parse("hello name1 name2 --switch --colors red green blue".Split(' '));

            command.Execute();

            Assert.AreEqual("name1, name2", parameterValue);
            Assert.True(switchValue);
            Assert.AreEqual("red, green, blue", optionValue);
        }
    }
}