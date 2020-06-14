using System;

using Kirkin.CommandLine;
using Kirkin.CommandLine.Help;

using NUnit.Framework;

namespace Kirkin.Tests.CommandLine
{
    public class CommandLineParsingTests
    {
        [Test]
        public void BasicCommandLineParsing()
        {
            CommandLineParser parser = new CommandLineParser {
                CaseInsensitive = true
            };

            parser.DefineCommand("zzz", zzz =>
            {
                zzz.AddOption("fizz");
                zzz.AddOption("buzz");
                zzz.AddSwitch("holy-moly", shortName: "hm");
            });

            string subscription = null;
            bool validate = false;

            parser.DefineCommand("sync", sync =>
            {
                sync.AddOption("subscription", "s");
                sync.AddSwitch("validate", "v");

                sync.Executed += (s, e) =>
                {
                    subscription = e.Args.GetOption("subscription");
                    validate = e.Args.GetSwitch("validate");
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

                Assert.AreEqual("1", command.Args.GetOption("fizz"));
                Assert.AreEqual("ultra", command.Args.GetOption("buzz"));
                Assert.True(command.Args.GetSwitch("holy-moly"));
            }
        }

        [Test]
        public void Parameters()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("sync", sync =>
            {
                sync.AddParameter("subscription");
                sync.AddSwitch("validate", shortName: "v");
            });

            ICommand command = parser.Parse("sync extra --validate".Split(' '));

            Assert.AreEqual("extra", (string)command.Args.All["subscription"]);
            Assert.AreEqual("extra", command.Args.GetParameter());
            Assert.True((bool)command.Args.All["validate"]);
            Assert.True(command.Args.GetSwitch("validate"));
        }

        [Test]
        public void DefaultValues()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("sync", sync =>
            {
                sync.AddParameter("subscription");
                sync.AddSwitch("validate", shortName: "v");
                sync.AddOption("log", shortName: "l");
            });

            {
                ICommand command = parser.Parse("sync --validate --log zzz.txt".Split(' '));

                Assert.Null(command.Args.All["subscription"]);
            }

            {
                ICommand command = parser.Parse("sync main --log zzz.txt".Split(' '));

                Assert.False((bool)command.Args.All["validate"]);
            }

            {
                ICommand command = parser.Parse("sync main --validate".Split(' '));

                Assert.Null(command.Args.All["log"]);
            }
        }

        [Test]
        public void OptionValueVariants()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("sync", sync =>
            {
                sync.AddParameter("subscription");
                sync.AddOption("log", shortName: "l");
            });

            {
                ICommand command = parser.Parse("sync --log zzz.txt".Split(' '));

                Assert.AreEqual("zzz.txt", command.Args.All["log"]);
            }

            //{
            //    ICommand command = parser.Parse("sync --log=zzz.txt".Split(' '));

            //    Assert.AreEqual("zzz.txt", command.Arguments["log"]);
            //}

            //{
            //    ICommand command = parser.Parse("sync --log:zzz.txt".Split(' '));

            //    Assert.AreEqual("zzz.txt", command.Arguments["log"]);
            //}
        }

        [Test]
        public void ToStringTest()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("sync", sync =>
            {
                sync.AddParameter("subscription");
                sync.AddSwitch("validate", shortName: "v");
                sync.AddOption("log", shortName: "l");
            });

            Assert.AreEqual("sync <subscription> [-v] [-l <arg>]", parser.Commands[0].ToString());
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
                hello.Help = "Says hello";

                hello.AddParameterList("names");
                hello.AddSwitch("switch");
                hello.AddOptionList("colors");

                hello.Executed += (s, e) =>
                {
                    parameterValue = string.Join(", ", e.Args.GetParameterList());
                    switchValue = (bool)e.Args.All["switch"];
                    optionValue = string.Join(", ", (string[])e.Args.All["colors"]);
                };
            });

            ICommand command = parser.Parse("hello name1 name2 --switch --colors red green blue".Split(' '));

            command.Execute();

            Assert.AreEqual("name1, name2", parameterValue);
            Assert.True(switchValue);
            Assert.AreEqual("red, green, blue", optionValue);
        }

        [Test]
        public void GeneralHelp()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("aaa", aaa => aaa.Help = "Does the zzz thing.");
            parser.DefineCommand("bbb", bbb => bbb.Help = "Does the uuu thing, which is totally different to the aaa thing. Totally.");
            parser.DefineCommand("ccc", ccc => ccc.Help = "Does the ccc thing.");

            ICommand command = parser.Parse("--help");

            command.Execute();

            Assert.True(parser.Parse("--help") is RootHelpCommand);
            Assert.True(parser.Parse("/?") is RootHelpCommand);
            Assert.True(parser.Parse(new string[0]) is RootHelpCommand);
            Assert.True(parser.Parse("") is RootHelpCommand);

            // Check text.
            string expected = @"Usage: Kirkin <command> [<args>].

    aaa    Does the zzz thing.
    bbb    Does the uuu thing, which is totally different to the aaa
           thing. Totally.
    ccc    Does the ccc thing.
";
            Assert.AreEqual(expected, ((IHelpCommand)command).RenderHelpText());
        }

        [Test]
        public void CommandHelp()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("command", c =>
            {
                c.AddParameter("aaa", help: "Does the zzz thing.");
                c.AddOption("bbb", help: "Does the uuu thing, which is totally different to the aaa thing. Totally.");
                c.AddSwitch("ccc", help: "Does the ccc thing.");
            });

            ICommand command = parser.Parse("command --help".Split(' '));

            command.Execute();

            Assert.True(parser.Parse("command --help".Split(' ')) is CommandDefinitionHelpCommand);
            Assert.True(parser.Parse("command /?".Split(' ')) is CommandDefinitionHelpCommand);
            Assert.True(parser.Parse("command -?".Split(' ')) is CommandDefinitionHelpCommand);
        }

        [Test]
        public void CommandHelpLongText()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("command", c =>
            {
                c.AddParameter("aaa", help: "Does the zzz thing.");
                c.AddOption("bbb", help: "Does the uuu thing, which is totally different to the aaa thing. Totally.");
                c.AddSwitch("ccc", help: "Does the ccc thing.");
            });

            ICommand command = parser.Parse("command --help".Split(' '));

            // Check text.
            string expected = @"Usage: Kirkin command <aaa> [--bbb <arg>] [--ccc].

    <aaa>          Does the zzz thing.
    --bbb <arg>    Does the uuu thing, which is totally different to the
                   aaa thing. Totally.
    --ccc          Does the ccc thing.
";
            Assert.AreEqual(expected, ((IHelpCommand)command).RenderHelpText());
        }

        [Test]
        public void CommandHelpShortText()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("command", c =>
            {
                c.AddParameter("aaa", help: "Does the zzz thing.");
                c.AddOption("bbb", "b", help: "Does the uuu thing, which is totally different to the aaa thing. Totally.");
                c.AddSwitch("ccc", "c", help: "Does the ccc thing.");
            });

            ICommand command = parser.Parse("command --help".Split(' '));

            // Check text.
            string expected = @"Usage: Kirkin command <aaa> [-b <arg>] [-c].

    <aaa>              Does the zzz thing.
    -b, --bbb <arg>    Does the uuu thing, which is totally different to
                       the aaa thing. Totally.
    -c, --ccc          Does the ccc thing.
";
            Assert.AreEqual(expected, ((IHelpCommand)command).RenderHelpText());
        }

        [Test]
        public void ParseSingleCommand()
        {
            CommandDefinition definition = new CommandDefinition();

            definition.AddParameterList("names");
            definition.AddSwitch("hru");

            ICommand command = definition.Parse("Stu Dru Gru --hru".Split(' '));

            Assert.True((bool)command.Args.All["hru"]);

            Console.WriteLine(string.Join(", ", (string[])command.Args.All["names"]));
        }

        [Test]
        public void PositionalArgsNoParam()
        {
            CommandDefinition definition = new CommandDefinition();

            definition.AddOption("a", positional: true);
            definition.AddOption("b", positional: true);
            definition.AddSwitch("c");
            definition.AddOption("d", positional: true);

            ICommand command = definition.Parse("aaa bbb --c --d ddd".Split(' '));

            Assert.AreEqual("aaa", command.Args.GetOption("a"));
            Assert.AreEqual("bbb", command.Args.GetOption("b"));
            Assert.True(command.Args.GetSwitch("c"));
            Assert.AreEqual("ddd", command.Args.GetOption("d"));
        }

        [Test]
        public void PositionalArgsWithParam()
        {
            CommandDefinition definition = new CommandDefinition();

            definition.AddParameter("a");
            definition.AddOption("b", positional: true);
            definition.AddSwitch("c");
            definition.AddOption("d", positional: true);

            ICommand command = definition.Parse("aaa bbb --c --d ddd".Split(' '));

            Assert.AreEqual("aaa", command.Args.GetParameter());
            Assert.AreEqual("bbb", command.Args.GetOption("b"));
            Assert.True(command.Args.GetSwitch("c"));
            Assert.AreEqual("ddd", command.Args.GetOption("d"));
        }

        [Test]
        public void PositionalArgsFailForMultiValues()
        {
            CommandDefinition definition = new CommandDefinition();

            definition.AddOption("a", positional: true);
            definition.AddOptionList("b", positional: true);

            ICommand command = definition.Parse(new[] { "aaa" });

            Assert.AreEqual("aaa", command.Args.GetOption("a"));

            Assert.Throws<ArgumentException>(() => definition.Parse(new[] { "aaa", "bbb" }));
        }

        [Test]
        public void MixedPositionalAndNonPositionalArgs()
        {
            CommandDefinition definition = new CommandDefinition();

            definition.AddOption("a", positional: true);
            definition.AddOption("b", positional: false);
            definition.AddOption("c", positional: true);

            ICommand command = definition.Parse("aaa ccc".Split(' '));

            Assert.AreEqual("aaa", command.Args.GetOption("a"));
            Assert.AreEqual("ccc", command.Args.GetOption("c"));

            command = definition.Parse("aaa ccc --b bbb".Split(' '));

            Assert.AreEqual("aaa", command.Args.GetOption("a"));
            Assert.AreEqual("bbb", command.Args.GetOption("b"));
            Assert.AreEqual("ccc", command.Args.GetOption("c"));
        }

        [Test]
        public void AppDetailsInHelpCommand()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.ShowAppDetailsInHelp = true;

            Console.WriteLine(((IHelpCommand)parser.Parse("--help")).RenderHelpText());
        }

        [Test]
        public void NestedCommandGroupExecute()
        {
            CommandLineParser parser = new CommandLineParser();
            bool bExecuted = false;

            parser.DefineCommand("aaa", builder => builder.DefineSubCommand("bbb", cmd => cmd.Executed += (s, e) => bExecuted = true));

            ICommand command = parser.Parse("aaa bbb".Split(' '));

            command.Execute();

            Assert.True(bExecuted);
        }

        [Test]
        public void NestedCommandGroupHelp()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("aaa", collection =>
            {
                collection.Help = "group aaa.";

                collection.AddSwitch("iii", help: "Switch iii.");

                collection.DefineSubCommand("bbb", cmd => cmd.Help = "command bbb.");
                collection.DefineSubCommand("ccc", cmd => cmd.Help = "command ccc.");

                collection.DefineSubCommand("ddd", ddd =>
                {
                    ddd.Help = "command group ddd.";

                    ddd.DefineSubCommand("eee", cmd => cmd.Help = "command eee.");
                });
            });

            ICommand command = parser.Parse("aaa --help".Split(' '));

            string expected = @"Usage: Kirkin aaa <command>.

    --iii    Switch iii.

    bbb    command bbb.
    ccc    command ccc.
    ddd    command group ddd.
";
            Assert.AreEqual(expected, ((IHelpCommand)command).RenderHelpText());

            command = parser.Parse("aaa bbb --help".Split(' '));

            Assert.AreEqual("Usage: Kirkin aaa bbb.", ((IHelpCommand)command).RenderHelpText());

            command = parser.Parse("aaa ddd --help".Split(' '));

            expected = @"Usage: Kirkin aaa ddd <command>.

    eee    command eee.
";
            Assert.AreEqual(expected, ((IHelpCommand)command).RenderHelpText());

            command = parser.Parse("aaa ddd eee --help".Split(' '));

            Assert.AreEqual("Usage: Kirkin aaa ddd eee.", ((IHelpCommand)command).RenderHelpText());
        }

        [Test]
        public void MixedCommandAndSubCommands()
        {
            CommandLineParser parser = new CommandLineParser();
            int commandExecuteCount = 0;
            int subCommandExecuteCount = 0;

            parser.DefineCommand("command", command =>
            {
                command.Help = "Command help.";

                command.Executed += (s, e) => commandExecuteCount++;

                command.DefineSubCommand("subcommand", subcommand =>
                {
                    subcommand.Help = "Subcommand help.";

                    subcommand.Executed += (s, e) => subCommandExecuteCount++;
                });
            });

            parser.Parse("command").Execute();

            Assert.AreEqual(1, commandExecuteCount);
            Assert.AreEqual(0, subCommandExecuteCount);

            parser.Parse("command --help".Split(' ')).Execute();

            Assert.AreEqual(1, commandExecuteCount);
            Assert.AreEqual(0, subCommandExecuteCount);

            parser.Parse("command subcommand".Split(' ')).Execute();

            Assert.AreEqual(1, commandExecuteCount);
            Assert.AreEqual(1, subCommandExecuteCount);

            parser.Parse("command subcommand --help".Split(' ')).Execute();

            Assert.AreEqual(1, commandExecuteCount);
            Assert.AreEqual(1, subCommandExecuteCount);
        }

        [Test]
        public void ParseExplicitlyNamedOptionValueWithSlashes()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("command", cmd =>
            {
                cmd.Help = "Command help.";

                cmd.AddParameter("arg");
                cmd.AddSwitch("test");
                cmd.AddOption("path", positional: true);
                cmd.AddOption("residual");
            });

            ICommand command = parser.Parse("command", "dummy", "--test", "--path", "/backups/db.bak", "--residual", "residualvalue");

            Assert.AreEqual(command.Args.GetParameter(), "dummy");
            Assert.True(command.Args.GetSwitch("test"));
            Assert.AreEqual(command.Args.GetOption("path"), "/backups/db.bak");
            Assert.AreEqual(command.Args.GetOption("residual"), "residualvalue");
        }

        [Test]
        public void ParseImplicitPositionalOptionValueWithSlashes()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("command", cmd =>
            {
                cmd.Help = "Command help.";

                cmd.AddParameter("arg");
                cmd.AddSwitch("test");
                cmd.AddOption("path", positional: true);
            });

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => parser.Parse("command", "dummy", "--test", "/backups/db.bak"));

            Assert.AreEqual(ex.Message, "Unknown option 'backups/db.bak'.");
        }

        [Test]
        public void ParseBenchmark()
        {
            CommandLineParser parser = new CommandLineParser();
            int count = 0;

            parser.DefineCommand("hello", command =>
                command.DefineSubCommand("great", great =>
                    great.DefineSubCommand("big", big =>
                        big.DefineSubCommand("white", white =>
                            white.DefineSubCommand("world", world => world.Executed += (s, e) => count++)
                        )
                    )
                )
            );

            string[] args = new[] { "hello", "great", "big", "white", "world" };

            for (int i = 0; i < 100000; i++) {
                parser.Parse(args).Execute();
            }
        }
    }
}