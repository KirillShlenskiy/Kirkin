#if NET_45 && !__MOBILE__

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Logging
{
    /// <summary>
    /// Simple monitor for log files which always grow.
    /// </summary>
    public sealed class LogFileMonitor
    {
        /// <summary>
        /// Full path to the log file monitored by this instance.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Time interval which must elapse with no new entries before
        /// all buffered entries are flushed to event subscribers.
        /// </summary>
        public TimeSpan Throttling { get; }

        /// <summary>
        /// Raised when the monitor detects a new line appended to the log file.
        /// Raised on the thread which calls MonitorAsync if the SynchronizationContext is not null.
        /// </summary>
        public event Action<string> LineRead;

        /// <summary>
        /// Creates a new instance of the monitor which reports
        /// any new lines written to the log file immediately.
        /// </summary>
        public LogFileMonitor(string filePath)
            : this(filePath, TimeSpan.Zero)
        {
        }

        /// <summary>
        /// Creates a new instance of the monitor which waits for the given interval
        /// with no new entries before flushing all buffered entries to event subscribers.
        /// </summary>
        public LogFileMonitor(string filePath, TimeSpan throttling)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("filePath");

            FilePath = filePath;
            Throttling = throttling;
        }

        /// <summary>
        /// Starts monitoring the file optionally checking the given cancellation token.
        /// </summary>
        public async Task MonitorAsync(CancellationToken ct = default(CancellationToken))
        {
            // 0 to read from start.
            // FileInfo.Length to read new entries only.
            int lastStreamPosition = (int)new FileInfo(FilePath).Length;

            // Watcher async reset event.
            TaskCompletionSource<bool> tcs = null;

            using (FileSystemWatcher fileWatcher = new FileSystemWatcher())
            {
                fileWatcher.Path = Path.GetDirectoryName(FilePath);
                fileWatcher.Filter = Path.GetFileName(FilePath);
                fileWatcher.EnableRaisingEvents = true;

                fileWatcher.Changed += (s, e) =>
                {
                    if (e.ChangeType == WatcherChangeTypes.Changed)
                    {
                        lock (fileWatcher)
                        {
                            if (tcs != null) {
                                tcs.TrySetResult(true);
                            }
                        }
                    }
                };

                CancellationTokenRegistration ctRegistration = ct.Register(() =>
                {
                    lock (fileWatcher)
                    {
                        if (tcs != null) {
                            tcs.TrySetCanceled();
                        };
                    }
                });

                using (ctRegistration)
                using (FileStream stream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    while (true)
                    {
                        if (Throttling != TimeSpan.Zero)
                        {
                            // Introduce a bit of throttling to batch things up better.
                            await Task.Delay(Throttling, ct);
                        }

                        ct.ThrowIfCancellationRequested();

                        if (stream.Length < lastStreamPosition) {
                            // File truncated.
                            break;
                        }

                        if (stream.Length == lastStreamPosition)
                        {
                            bool awaitNeeded = false;

                            lock (fileWatcher)
                            {
                                if (stream.Length == lastStreamPosition)
                                {
                                    tcs = new TaskCompletionSource<bool>();
                                    awaitNeeded = true;
                                }
                            }

                            if (awaitNeeded)
                            {
                                // No change. Wait for the watcher to tell us something.
                                Stopwatch sw = Stopwatch.StartNew();

                                await tcs.Task;

                                Debug.Print("Await duration: {0:0.000} s.", (double)sw.ElapsedMilliseconds / 1000);
                            }
                        }

                        // New data available.
                        stream.Position = lastStreamPosition;

                        int capacity = Math.Max(1024, (int)(stream.Length - stream.Position) * 2);

                        using (MemoryStream ms = new MemoryStream(capacity))
                        {
                            int positionBeforeCopy = lastStreamPosition;

                            await stream.CopyToAsync(ms);

                            ms.Position = 0;

                            // StreamReader closes the underlying stream
                            // on Dispose, so we can't use it on live stream.
                            using (StreamReader reader = new StreamReader(ms))
                            {
                                while (true)
                                {
                                    string line = await reader.ReadLineAsync();

                                    ct.ThrowIfCancellationRequested();

                                    if (line == null) {
                                        break;
                                    }

                                    if (LineRead != null) {
                                        LineRead(line);
                                    }

                                    // Successfully reported line.
                                    lastStreamPosition = positionBeforeCopy + (int)ms.Position;
                                }

                                if (ms.Capacity != capacity) {
                                    Debug.Print("ms had to be resized. New capacity: {0}.", ms.Capacity);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

#endif