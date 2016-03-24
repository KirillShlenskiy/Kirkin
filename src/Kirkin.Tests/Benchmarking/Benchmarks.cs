using System;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Kirkin.Tests.Benchmarking
{
    /// <summary>
    /// Benchmarking helpers.
    /// </summary>
    public static class Benchmarks
    {
        /// <summary>
        /// Default benchmark configuration.
        /// </summary>
        private static IConfig DefaultBenchmarkConfig { get; } = CreateDefaultConfig();

        private static IConfig CreateDefaultConfig()
        {
            ManualConfig config = new ManualConfig();

            config.Add(Job.Default);
            config.Add(DefaultConfig.Instance.GetColumns().ToArray());

            return config;
        }

        /// <summary>
        /// Executes benchmarks defined by the given type and
        /// produces a human-readable report from the benchmark summary.
        /// </summary>
        public static string Run<T>()
        {
            return Run<T>(DefaultBenchmarkConfig);
        }

        /// <summary>
        /// Executes benchmarks defined by the given type and
        /// produces a human-readable report from the benchmark summary.
        /// </summary>
        public static string Run<T>(IConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            Summary summary = BenchmarkRunner.Run<T>(config);

            return ExportSummary(summary);
        }

        /// <summary>
        /// Produces a human-readable report from the given benchmark summary.
        /// </summary>
        private static string ExportSummary(Summary summary)
        {
            StringBuilderLogger logger = new StringBuilderLogger();

            MarkdownExporter.Default.ExportToLog(summary, logger);

            return logger.ToString();
        }

        sealed class StringBuilderLogger : ILogger
        {
            private readonly StringBuilder StringBuilder = new StringBuilder();

            public void Write(LogKind logKind, string text)
            {
                StringBuilder.Append(text);
            }

            public void Write(LogKind logKind, string format, params object[] args)
            {
                StringBuilder.AppendFormat(format, args);
            }

            public void WriteLine()
            {
                StringBuilder.AppendLine();
            }

            public void WriteLine(LogKind logKind, string text)
            {
                StringBuilder.AppendLine(text);
            }

            public override string ToString()
            {
                return StringBuilder.ToString();
            }
        }
    }
}