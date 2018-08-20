using System;

using Kirkin.CommandLine;
using Kirkin.CommandLine.Commands;
using Kirkin.CommandLine.Commands.Help;

using NUnit.Framework;

namespace Kirkin.Tests.CommandLine
{
    public class CommandLineTests
    {
        [Test]
        public void BasicCommandLineParsing()
        {
            CommandLineParser parser = new CommandLineParser {
                CaseInsensitive = true
            };

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

                Assert.AreEqual("1", command.Arguments.GetOption("fizz"));
                Assert.AreEqual("ultra", command.Arguments.GetOption("buzz"));
                Assert.True(command.Arguments.GetSwitch("holy-moly"));
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

            Assert.AreEqual("extra", (string)command.Arguments.All["subscription"]);
            Assert.AreEqual("extra", command.Arguments.GetParameter());
            Assert.True((bool)command.Arguments.All["validate"]);
            Assert.True(command.Arguments.GetSwitch("validate"));
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

                Assert.Null(command.Arguments.All["subscription"]);
            }

            {
                ICommand command = parser.Parse("sync main --log zzz.txt".Split(' '));

                Assert.False((bool)command.Arguments.All["validate"]);
            }

            {
                ICommand command = parser.Parse("sync main --validate".Split(' '));

                Assert.Null(command.Arguments.All["log"]);
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

                Assert.AreEqual("zzz.txt", command.Arguments.All["log"]);
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
                sync.DefineParameter("subscription");
                sync.DefineSwitch("validate", shortName: "v");
                sync.DefineOption("log", shortName: "l");
            });

            Assert.AreEqual("sync <subscription> [-v] [-l <arg>]", parser.CommandDefinitions[0].ToString());
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

                hello.DefineParameterList("names");
                hello.DefineSwitch("switch");
                hello.DefineOptionList("colors");

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

            // Not help commands:
            Assert.Throws<InvalidOperationException>(() => parser.Parse(new string[0]));
            Assert.Throws<InvalidOperationException>(() => parser.Parse(""));

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
                c.DefineParameter("aaa", help: "Does the zzz thing.");
                c.DefineOption("bbb", help: "Does the uuu thing, which is totally different to the aaa thing. Totally.");
                c.DefineSwitch("ccc", help: "Does the ccc thing.");
            });

            ICommand command = parser.Parse("command --help".Split(' '));

            command.Execute();

            Assert.True(parser.Parse("command --help".Split(' ')) is CommandDefinitionHelpCommand);
            Assert.True(parser.Parse("command /?".Split(' ')) is CommandDefinitionHelpCommand);

            // Not help commands:
            Assert.Throws<InvalidOperationException>(() => parser.Parse(new string[0]));
            Assert.Throws<InvalidOperationException>(() => parser.Parse(""));
        }

        [Test]
        public void CommandHelpLongText()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("command", c =>
            {
                c.DefineParameter("aaa", help: "Does the zzz thing.");
                c.DefineOption("bbb", help: "Does the uuu thing, which is totally different to the aaa thing. Totally.");
                c.DefineSwitch("ccc", help: "Does the ccc thing.");
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
                c.DefineParameter("aaa", help: "Does the zzz thing.");
                c.DefineOption("bbb", "b", help: "Does the uuu thing, which is totally different to the aaa thing. Totally.");
                c.DefineSwitch("ccc", "c", help: "Does the ccc thing.");
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
            IndividualCommandDefinition definition = new IndividualCommandDefinition();

            definition.DefineParameterList("names");
            definition.DefineSwitch("hru");

            ICommand command = definition.Parse("Stu Dru Gru --hru".Split(' '));

            Assert.True((bool)command.Arguments.All["hru"]);

            Console.WriteLine(string.Join(", ", (string[])command.Arguments.All["names"]));
        }

        [Test]
        public void PositionalArgsNoParam()
        {
            IndividualCommandDefinition definition = new IndividualCommandDefinition();

            definition.DefineOption("a", positional: true);
            definition.DefineOption("b", positional: true);
            definition.DefineSwitch("c", positional: true);
            definition.DefineOption("d", positional: true);

            ICommand command = definition.Parse("aaa bbb --c --d ddd".Split(' '));

            Assert.AreEqual("aaa", command.Arguments.GetOption("a"));
            Assert.AreEqual("bbb", command.Arguments.GetOption("b"));
            Assert.True(command.Arguments.GetSwitch("c"));
            Assert.AreEqual("ddd", command.Arguments.GetOption("d"));
        }

        [Test]
        public void PositionalArgsWithParam()
        {
            IndividualCommandDefinition definition = new IndividualCommandDefinition();

            definition.DefineParameter("a");
            definition.DefineOption("b", positional: true);
            definition.DefineSwitch("c", positional: true);
            definition.DefineOption("d", positional: true);

            ICommand command = definition.Parse("aaa bbb --c --d ddd".Split(' '));

            Assert.AreEqual("aaa", command.Arguments.GetParameter());
            Assert.AreEqual("bbb", command.Arguments.GetOption("b"));
            Assert.True(command.Arguments.GetSwitch("c"));
            Assert.AreEqual("ddd", command.Arguments.GetOption("d"));
        }

        [Test]
        public void PositionalArgsFailForMultiValues()
        {
            IndividualCommandDefinition definition = new IndividualCommandDefinition();

            definition.DefineOption("a", positional: true);
            definition.DefineOptionList("b", positional: true);

            ICommand command = definition.Parse(new[] { "aaa" });

            Assert.AreEqual("aaa", command.Arguments.GetOption("a"));

            Assert.Throws<ArgumentException>(() => definition.Parse(new[] { "aaa", "bbb" }));
        }

        [Test]
        public void MixedPositionalAndNonPositionalArgs()
        {
            IndividualCommandDefinition definition = new IndividualCommandDefinition();

            definition.DefineOption("a", positional: true);
            definition.DefineOption("b", positional: false);
            definition.DefineOption("c", positional: true);

            ICommand command = definition.Parse("aaa ccc".Split(' '));

            Assert.AreEqual("aaa", command.Arguments.GetOption("a"));
            Assert.AreEqual("ccc", command.Arguments.GetOption("c"));

            command = definition.Parse("aaa ccc --b bbb".Split(' '));

            Assert.AreEqual("aaa", command.Arguments.GetOption("a"));
            Assert.AreEqual("bbb", command.Arguments.GetOption("b"));
            Assert.AreEqual("ccc", command.Arguments.GetOption("c"));
        }

        [Test]
        public void AppDetailsInHelpCommand()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.ShowAppDetailsInHelp = true;

            parser.Parse("--help").Execute();
        }

        [Test]
        public void NestedCommandGroupExecute()
        {
            CommandLineParser parser = new CommandLineParser();
            bool bExecuted = false;

            parser.DefineCommandGroup("aaa", builder => builder.DefineCommand("bbb", cmd => cmd.Executed += (s, e) => bExecuted = true));

            ICommand command = parser.Parse("aaa bbb".Split(' '));

            command.Execute();

            Assert.True(bExecuted);
        }

        [Test]
        public void NestedCommandGroupHelp()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommandGroup("aaa", group =>
            {
                group.Help = "group aaa.";

                group.DefineCommand("bbb", cmd => cmd.Help = "command bbb.");
                group.DefineCommand("ccc", cmd => cmd.Help = "command ccc.");
            });

            ICommand command = parser.Parse("aaa --help".Split(' '));

            string expected = @"Usage: Kirkin aaa <command>.

    bbb    command bbb.
    ccc    command ccc.
";

            Assert.AreEqual(expected, ((IHelpCommand)command).RenderHelpText());
        }
    }
}