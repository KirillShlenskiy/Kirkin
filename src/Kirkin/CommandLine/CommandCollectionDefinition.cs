//using System;
//using System.Collections.Generic;

//using Kirkin.CommandLine.Help;

//namespace Kirkin.CommandLine
//{
//    /// <summary>
//    /// Container for multiple logically grouped commands.
//    /// </summary>
//    public sealed class CommandCollectionDefinition : CommandDefinition, ICommandDefinitionContainer
//    {
//        internal CommandCollectionDefinition(string name, CommandDefinition parent, bool caseInsensitive)
//            : base(name, parent)
//        {
//            Parser = new CommandLineParser {
//                CaseInsensitive = caseInsensitive,
//                Parent = this
//            };
//        }

//        /// <summary>
//        /// Parses the command line args and returns the configured, ready-to-execute command.
//        /// </summary>
//        internal override ICommand Parse(string[] args)
//        {
//            if (args.Length == 0 || (args.Length == 1 && CommandSyntax.IsHelpSwitch(args[0], StringEqualityComparer))) {
//                return new CommandCollectionDefinitionHelpCommand(this);
//            }

//            return Parser.Parse(args);
//        }

//        private protected override IHelpCommand CreateHelpCommand()
//        {
//            return new CommandCollectionDefinitionHelpCommand(this);
//        }

//        /// <summary>
//        /// Returns the string description of this command group.
//        /// </summary>
//        public override string ToString()
//        {
//            return $"{Name} <command>";
//        }
//    }
//}