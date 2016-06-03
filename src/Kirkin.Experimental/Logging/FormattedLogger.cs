using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kirkin.Logging
{
    // Experimental.
    internal sealed class FormattedLogger : Logger
    {
        private readonly Logger Inner;
        private readonly List<IEntryFormatter> __formatters = new List<IEntryFormatter>();

        public IList<IEntryFormatter> Formatters
        {
            get
            {
                return __formatters;
            }
        }

        public FormattedLogger(Logger inner)
        {
            Inner = inner;
        }

        protected override void LogEntry(string entry)
        {
            Action<string> logDelegate = EntryFormatter.DecorateLogEntryDelegateWithFormatters(Inner.Log, __formatters.ToArray());

            logDelegate(entry);
        }
    }
}
