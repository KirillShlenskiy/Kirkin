using System.Collections.Generic;

using Kirkin.Logging;

using NUnit.Framework;

namespace Kirkin.Tests.Logging
{
    public class LogEntryBlockTests
    {
        [Test]
        public void EntryBlockPerf()
        {
            Logger logger = Logger.Null;

            for (int i = 0; i < 10000000; i++)
            {
                using (new LogEntryBlock(logger))
                {
                }
            }
        }

        [Test]
        public void BasicEntryBlockStartingWithNull()
        {
            var logger = new ListLogger();

            using (new LogEntryBlock(logger))
            {
                logger.Log("a");
                logger.Log("b");
            }

            Assert.AreEqual(new[] { "a", "b", "" }, logger.Entries);
        }

        [Test]
        public void BasicEntryBlockStartingWithEmpty()
        {
            var logger = new ListLogger();

            logger.Log("");

            using (new LogEntryBlock(logger))
            {
                logger.Log("a");
                logger.Log("b");
            }

            Assert.AreEqual(new[] { "", "a", "b", "" }, logger.Entries);
        }

        [Test]
        public void BasicEntryBlockStartingWithNonEmpty()
        {
            var logger = new ListLogger();

            logger.Log("z");

            using (new LogEntryBlock(logger))
            {
                logger.Log("a");
                logger.Log("b");
            }

            Assert.AreEqual(new[] { "z", "", "a", "b", "" }, logger.Entries);
        }

        [Test]
        public void CompleteOnlyCreatesOneEmptyEntry()
        {
            var logger = new ListLogger();

            using (var block = new LogEntryBlock(logger))
            {
                logger.Log("a");
                block.Complete();
                block.Complete();
            }

            Assert.AreEqual(new[] { "a", "" }, logger.Entries);
        }

        [Test]
        public void CompleteWorksAgainAfternNonEmptyEntry()
        {
            var logger = new ListLogger();

            using (var block = new LogEntryBlock(logger))
            {
                logger.Log("a");
                block.Complete();
                logger.Log("a");
                block.Complete();
            }

            Assert.AreEqual(new[] { "a", "", "a", "" }, logger.Entries);
        }

        [Test]
        public void EmptyBlockProducesSingleEmptyEntry()
        {
            var logger = new ListLogger();

            logger.Log("a");

            using (new LogEntryBlock(logger))
            {
            }

            logger.Log("a");

            Assert.AreEqual(new[] { "a", "", "a" }, logger.Entries);
        }

        sealed class ListLogger : Logger
        {
            public List<string> Entries { get; } = new List<string>();

            protected override void LogEntry(string entry)
            {
                Entries.Add(entry);
            }
        }
    }
}