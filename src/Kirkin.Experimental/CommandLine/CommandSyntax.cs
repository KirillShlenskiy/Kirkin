using System;

namespace Kirkin.CommandLine
{
    public sealed class CommandSyntax
    {
        public string Name { get; }

        public event Action Invoked;

        internal CommandSyntax(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Command name cannot be empty.");

            Name = name;
        }

        public void DefineOption(string name, string shortVersion, ref string value)
        {
            throw new NotImplementedException();
        }

        public void DefineOption(string name, string shortVersion, ref bool value)
        {
            throw new NotImplementedException();
        }

        public void DefineOption(string name, string shortVersion, ref int value)
        {
            throw new NotImplementedException();
        }

        public void DefineOptionList(string shortOption, string longOption) // string[].
        {
            throw new NotImplementedException();
        }

        public void DefineParameter(string name, ref string value, string help = null)
        {
            throw new NotImplementedException();
        }

        public void DefineParameter(string name, ref int value, string help = null)
        {
            throw new NotImplementedException();
        }

        public void DefineParameter<T>(string name, ref T value, Func<string, T> valueConverter, string help = null)
        {
            throw new NotImplementedException();
        }

        internal ICommand BuildCommand()
        {
            throw new NotImplementedException();
        }
    }
}