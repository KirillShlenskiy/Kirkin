using Kirkin.CommandLine;

using NUnit.Framework;

namespace Kirkin.Tests.CommandLine
{
    public class CommandLineTests
    {
        [Test]
        [Ignore("Work in progress")]
        public void BasincCommandLineParsing()
        {
            CommandLineParser parser = new CommandLineParser();

            parser.DefineCommand("sync", sync =>
            {
                bool verify = false;
                string subscription = null;

                sync.DefineOption("verify", "v", ref verify);
                sync.DefineParameter("subscription", ref subscription);

                return () =>
                {
                    if (verify)
                    {

                    }
                    else
                    {

                    }
                };
            });

            ICommand command = parser.Parse("sync main --validate");

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