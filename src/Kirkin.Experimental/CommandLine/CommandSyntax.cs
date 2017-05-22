using System;
using System.Collections.Generic;
using System.Linq;

namespace Kirkin.CommandLine
{
    public sealed class CommandSyntax
    {
        private readonly Dictionary<string, Action<string[]>> _tokensByFullName = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Action<string[]>> _tokensByShortName = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase);

        public string Name { get; }

        internal CommandSyntax(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Command name cannot be empty.");

            Name = name;
        }

        public Func<T> DefineOption<T>(string name, string shortName, Func<string[], T> valueConverter)
        {
            T value = default(T);
            bool valueGenerated = false;

            Action<string[]> parse = args =>
            {
                value = valueConverter(args);
                valueGenerated = true;
            };

            _tokensByFullName.Add(name, parse);

            if (!string.IsNullOrEmpty(shortName)) _tokensByShortName.Add(shortName, parse);

            return () =>
            {
                if (valueGenerated) {
                    return value;
                }

                throw new InvalidOperationException("Value not yet generated.");
            };
        }

        //public void DefineOption(string name, string shortName, Action<string> setter)
        //{
        //    Action<string[]> parse = args => setter(args.Single());

        //    _tokensByFullName.Add(name, parse);

        //    if (!string.IsNullOrEmpty(shortName)) _tokensByShortName.Add(shortName, parse);
        //}

        //public void DefineOption(string name, string shortName, Action<bool> setter)
        //{
        //    Dictionary<string, bool> boolValues = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase) {
        //        { "0", false },
        //        { "1", true },
        //        { "false", false },
        //        { "true", true }
        //    };

        //    Action<string[]> parse = args => setter(boolValues[args.Single()]);

        //    _tokensByFullName.Add(name, parse);

        //    if (!string.IsNullOrEmpty(shortName)) _tokensByShortName.Add(shortName, parse);
        //}

        //public void DefineOption(string name, string shortName, Action<int> setter)
        //{
        //    Action<string[]> parse = args => setter(int.Parse(args.Single()));

        //    _tokensByFullName.Add(name, parse);

        //    if (!string.IsNullOrEmpty(shortName)) _tokensByShortName.Add(shortName, parse);
        //}

        //public void DefineOptionList(string shortOption, string longOption) // string[].
        //{
        //    throw new NotImplementedException();
        //}

        //public void DefineParameter(string name, Action<string> setter, string help = null)
        //{
        //    Action<string[]> parse = args => setter(args.Single());

        //    _tokensByFullName.Add(name, parse);
        //}

        //public void DefineParameter(string name, ref int value, string help = null)
        //{
        //    throw new NotImplementedException();
        //}

        //public void DefineParameter<T>(string name, ref T value, Func<string, T> valueConverter, string help = null)
        //{
        //    throw new NotImplementedException();
        //}

        internal void BuildCommand(string[] args) // ArraySlice<string>?
        {
            List<List<string>> chunks = new List<List<string>>();
            List<string> currentChunk = null;

            foreach (string arg in args)
            {
                if (currentChunk == null || arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    currentChunk = new List<string>();

                    chunks.Add(currentChunk);
                }

                currentChunk.Add(arg);
            }

            foreach (List<string> chunk in chunks)
            {
                Action<string[]> token = null;

                if (chunk[0].StartsWith("-") || chunk[0].StartsWith("/"))
                {
                    // Option.
                    if (chunk[0].StartsWith("--"))
                    {
                        string fullName = chunk[0].Substring(2);

                        if (!_tokensByFullName.TryGetValue(fullName, out token)) {
                            throw new InvalidOperationException($"Unable to find option with name '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("/"))
                    {
                        string fullName = chunk[0].Substring(1);

                        if (!_tokensByFullName.TryGetValue(fullName, out token)) {
                            throw new InvalidOperationException($"Unable to find option with name '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("-"))
                    {
                        string shortName = chunk[0].Substring(1);

                        if (!_tokensByShortName.TryGetValue(shortName, out token)) {
                            throw new InvalidOperationException($"Unable to find option with short name '{shortName}'.");
                        }
                    }
                }
                
                if (token == null) {
                    throw new InvalidOperationException($"Unhandled syntax token: '{chunk[0]}'.");
                }

                token(chunk.Skip(1).ToArray());
            }
        }
    }
}