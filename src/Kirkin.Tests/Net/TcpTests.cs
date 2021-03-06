﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Net;

using NUnit.Framework;

namespace Kirkin.Tests.Net
{
    public class TcpTests
    {
        const int PortNumber = 4444;

        [Test]
        public async Task MiniServer()
        {
            TaskCompletionSource<bool> serverRunning = new TaskCompletionSource<bool>();

            Task serverTask = Task.Run(async () =>
            {
                TcpServer server = new TcpServer(new IPEndPoint(IPAddress.Any, PortNumber));
                CancellationTokenSource cts = new CancellationTokenSource();

                Task runTask = server.RunAsync(async client =>
                {
                    NetworkStream stream = client.GetStream();

                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen: true))
                    {
                        while (true)
                        {
                            string line = await reader.ReadLineAsync().ConfigureAwait(false);

                            if (line == null) {
                                break;
                            }

                            Console.WriteLine($"[Received] {line}");

                            if (string.Equals(line, "Goodbye", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("Exiting");

                                cts.Cancel();

                                return;
                            }
                        }
                    }
                }, cts.Token);

                serverRunning.SetResult(true);

                await runTask.ConfigureAwait(false);
            });

            Task clientTask = Task.Run(async () =>
            {
                await serverRunning.Task.ConfigureAwait(false);

                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync("localhost", PortNumber).ConfigureAwait(false);

                    NetworkStream stream = client.GetStream();

                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, 4096, leaveOpen: true))
                    {
                        await writer.WriteLineAsync("Hello").ConfigureAwait(false);
                        await writer.FlushAsync().ConfigureAwait(false);
                        await Task.Delay(1000).ConfigureAwait(false);
                        await writer.WriteLineAsync("Goodbye").ConfigureAwait(false);
                        await writer.FlushAsync().ConfigureAwait(false);
                    }
                }
            });

            await Task.WhenAll(serverTask, clientTask).ConfigureAwait(false);
        }
    }
}