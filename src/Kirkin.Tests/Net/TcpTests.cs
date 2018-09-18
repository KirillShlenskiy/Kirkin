using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
                TcpListener server = new TcpListener(IPAddress.Any, PortNumber);

                server.Start();

                try
                {
                    serverRunning.SetResult(true);

                    while (true)
                    {
                        using (TcpClient client = await server.AcceptTcpClientAsync().ConfigureAwait(false))
                        {
                            NetworkStream stream = client.GetStream();

                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen: true))
                            {
                                while (true)
                                {
                                    string line = await reader.ReadLineAsync();

                                    if (line == null) {
                                        break;
                                    }

                                    Console.WriteLine($"[Received] {line}");

                                    if (string.Equals(line, "Goodbye", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Console.WriteLine("Exiting");

                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    server.Stop();
                }
            });

            Task clientTask = Task.Run(async () =>
            {
                await serverRunning.Task.ConfigureAwait(false);

                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(IPAddress.Loopback, PortNumber).ConfigureAwait(false);

                    NetworkStream stream = client.GetStream();

                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, 4096, leaveOpen: true))
                    {
                        await writer.WriteLineAsync("Hello");
                        await writer.FlushAsync();
                        await Task.Delay(1000);
                        await writer.WriteLineAsync("Goodbye");
                        await writer.FlushAsync();
                    }
                }
            });

            await Task.WhenAll(serverTask, clientTask);
        }
    }
}