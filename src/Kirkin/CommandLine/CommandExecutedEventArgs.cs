using System;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command execution event data.
    /// </summary>
    [Serializable]
    public class CommandExecutedEventArgs : EventArgs
    {
        /// <summary>
        /// The command being executed.
        /// </summary>
        public ICommand Command { get; }

        /// <summary>
        /// Command argument values.
        /// </summary>
        public CommandArguments Args { get; }

        /// <summary>
        /// Creates a new <see cref="CommandExecutedEventArgs"/> instance.
        /// </summary>
        internal CommandExecutedEventArgs(ICommand command, CommandArguments args)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (args == null) throw new ArgumentNullException(nameof(args));

            Command = command;
            Args = args;
        }
    }
}