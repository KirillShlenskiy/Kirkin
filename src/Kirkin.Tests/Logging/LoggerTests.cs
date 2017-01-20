using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using Kirkin.Logging;

using NUnit.Framework;

namespace Kirkin.Tests.Logging
{
    public class LoggerTests
    {
        [Test]
        public void BuilderApi()
        {
            Logger logger = new LoggerBuilder()
                .AddFormatter(EntryFormatter.SplitMultilineEntries)
                .AddFilter(entries => entries.Select(e => "zzz" + e))
                .AddFormatter(EntryFormatter.LogTimeBetweenEntries())
                .BuildLogger();
        }

        [Test]
        public void Formatters()
        {
            string output = "";
            Logger outputWriter = Logger.Create(e => output = output + e + Environment.NewLine);

            // Test unordered transform.
            Logger logger = new LoggerBuilder(outputWriter) {
                Formatters = {
                    EntryFormatter.Transform(s => "123" + s),
                    EntryFormatter.Transform(s => s + "321")
                }
            }
            .BuildLogger();

            logger.Log("Blah");

            Assert.AreEqual("123Blah321" + Environment.NewLine, output);

            // Test ordered transform.
            output = "";

            logger = new LoggerBuilder()
                .AddFormatter(EntryFormatter.SplitMultilineEntries)
                .AddFormatter(EntryFormatter.Transform(s => "zzz " + s))
                .AddLogger(outputWriter)
                .BuildLogger();

            logger.Log("This" + Environment.NewLine + "is" + Environment.NewLine + "awesome");

            Assert.AreEqual("zzz This" + Environment.NewLine + "zzz is" + Environment.NewLine + "zzz awesome" + Environment.NewLine, output);
        }

        [Test]
        public void CombineTest()
        {
            var entries = new List<string>();

            var logger = Logger.Combine(
                Logger.Create(s => entries.Add(s)),
                // Same as the other logger, but will reverse each entry.
                Logger.Create(s => entries.Add(new string(s.Reverse().ToArray())))
            );

            logger.Log("123");

            Assert.AreEqual(2, entries.Count);
            Assert.AreEqual("123", entries[0]);
            Assert.AreEqual("321", entries[1]);
        }

        [Test]
        public void CustomLoggerTest()
        {
            var entries = new List<string>();
            var logger = Logger.Create(s => entries.Add(s));

            Assert.True(entries.Count == 0);

            logger.Log("Entry 1");

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("Entry 1", entries.Last());

            logger.Log("Entry 2");

            Assert.AreEqual(2, entries.Count);
            Assert.AreEqual("Entry 2", entries.Last());
        }

        [Test]
        public void LogTimeBetweenEntriesCustomFormat()
        {
            var lines = new List<string>();
            var logger = Logger.Create(lines.Add).WithFormatters(EntryFormatter.LogTimeBetweenEntries("{0:0}"));

            logger.Log("1"); // Produces 1 line.
            Thread.Sleep(200);
            logger.Log("200 ms later."); // Produces 2 lines.

            Assert.AreEqual("1", lines[0]);
            Assert.AreEqual("0", lines[1]);
            Assert.AreEqual("200 ms later.", lines[2]);
        }

        [Test]
        public void LogTimeBetweenEntriesDefaultFormat()
        {
            var lines = new List<string>();
            var logger = Logger.Create(lines.Add).WithFormatters(EntryFormatter.LogTimeBetweenEntries());

            logger.Log("1"); // Produces 1 line.
            Thread.Sleep(200);
            logger.Log("200 ms later."); // Produces 2 lines.

            Assert.AreEqual("1", lines[0]);
            Assert.True(Regex.IsMatch(lines[1], @"\[Time elapsed: [0-9.]{5} s\.\]"));
            Assert.AreEqual("200 ms later.", lines[2]);
        }

        [Test]
        public void SplitLinesLoggerTest()
        {
            var entries = new List<string>();
            var logger = Logger.Create(entries.Add).WithFormatters(EntryFormatter.SplitMultilineEntries);

            logger.Log("Line 1" + Environment.NewLine + "Line 2" + Environment.NewLine + "Line 3");

            Assert.AreEqual(3, entries.Count);
            Assert.AreEqual("Line 1", entries[0]);
            Assert.AreEqual("Line 2", entries[1]);
            Assert.AreEqual("Line 3", entries[2]);
        }

        [Test]
        public void TimestampedLoggerTest()
        {
            var entry = default(string);
            var logger = Logger.Create(s => entry = s).WithFormatters(EntryFormatter.TimestampNonEmptyEntries("HH:mm:ss"));

            logger.Log("Entry 1");

            var datePortion = entry.Substring(0, "HH:mm:ss".Length);

            DateTime.ParseExact(datePortion, "HH:mm:ss", null);
        }

        [Test]
        public void WithOptionsTest()
        {
            var lines = new List<string>();

            var logger = new LoggerBuilder(lines.Add)
                .AddFormatter(EntryFormatter.LogTimeBetweenEntries())
                .AddFormatter(EntryFormatter.SplitMultilineEntries)
                .AddFormatter(EntryFormatter.TimestampNonEmptyEntries())
                .BuildLogger();

            logger.Log("Zzz" + Environment.NewLine + "Aaa"); // Produces 2 lines because of multiline split.
            logger.Log("This should be timed"); // Produces 2 lines because of time logging.

            Debug.Print(string.Join(Environment.NewLine, lines));
            Assert.AreEqual(4, lines.Count);

            string timeFormat = "HH:mm:ss";

            Func<string, bool> startsWithTime = s =>
            {
                if (s.Length < timeFormat.Length) {
                    return false;
                }

                DateTime date;
                string timeString = s.Substring(0, timeFormat.Length);

                return DateTime.TryParseExact(timeString, timeFormat, null, System.Globalization.DateTimeStyles.None, out date);
            };

            Assert.False(startsWithTime(""));
            Assert.True(startsWithTime(lines[0]));
            Assert.True(startsWithTime(lines[1]));
            Assert.True(startsWithTime(lines[2]));
            Assert.True(startsWithTime(lines[3]));
        }

        [Test]
        public void WithTransformationTest1()
        {
            // Reimplement timestamp logger.
            var entry = default(string);
            var logger = Logger.Create(s => entry = s).WithFormatters(EntryFormatter.Transform(s => DateTime.Now.ToString("HH:mm:ss") + " " + s));

            logger.Log("Entry 1");

            var datePortion = entry.Substring(0, "HH:mm:ss".Length);

            DateTime.ParseExact(datePortion, "HH:mm:ss", null);
        }

        [Test]
        public void WithTransformationTest2()
        {
            // Reimplement split line logger.
            var buffer = new List<string>();

            var logger = new LoggerBuilder(buffer.Add)
                .AddFilter(entries => entries.SelectMany(s => s.Split('\n').Select(l => l.Replace("\r", string.Empty))))
                .BuildLogger();

            logger.Log("Line 1" + Environment.NewLine + "Line 2" + Environment.NewLine + "Line 3");

            Assert.AreEqual(3, buffer.Count);
            Assert.AreEqual("Line 1", buffer[0]);
            Assert.AreEqual("Line 2", buffer[1]);
            Assert.AreEqual("Line 3", buffer[2]);
        }

        [Test]
        public void WithTransformationTest3()
        {
            // Reimplement timestamp logger.
            var entry = default(string);

            var logger = Logger.Create(s => entry = s)
                .WithFormatters(EntryFormatter.Transform(s => "1" + s))
                .WithFormatters(EntryFormatter.Transform(s => "2" + s))
                .WithFormatters(EntryFormatter.Transform(s => "3" + s));

            logger.Log("BOOM");

            Assert.AreEqual("123BOOM", entry);
        }
    }
}