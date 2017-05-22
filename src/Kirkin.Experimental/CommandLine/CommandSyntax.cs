using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine
{
    public sealed class CommandSyntax
    {
        private readonly Dictionary<string, CommandSyntaxToken> _tokensByFullName = new Dictionary<string, CommandSyntaxToken>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, CommandSyntaxToken> _tokensByShortName = new Dictionary<string, CommandSyntaxToken>(StringComparer.OrdinalIgnoreCase);

        abstract class CommandSyntaxToken
        {
        }

        class CommandSyntaxToken<T> : CommandSyntaxToken
        {
            internal Func<string, T> ValueConverter;
        }

        public string Name { get; }

        internal CommandSyntax(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Command name cannot be empty.");

            Name = name;
        }

        public void DefineOption(string name, string shortName, ref string value)
        {
            CommandSyntaxToken<string> token = new CommandSyntaxToken<string> { ValueConverter = s => s };

            _tokensByFullName.Add(name, token);

            if (!string.IsNullOrEmpty(shortName)) _tokensByShortName.Add(shortName, token);
        }

        public void DefineOption(string name, string shortName, ref bool value)
        {
            throw new NotImplementedException();
        }

        public void DefineOption(string name, string shortName, ref int value)
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

        internal ICommand BuildCommand(string[] args) // ArraySlice<string>?
        {
            throw new NotImplementedException();
        }
    }
}