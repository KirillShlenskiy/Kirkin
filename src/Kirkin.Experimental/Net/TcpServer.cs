using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Threading.Tasks;

namespace Kirkin.Net
{
    public sealed class TcpServer
    {
        public IPEndPoint Endpoint { get; }

        public TcpServer(IPEndPoint endpoint)
        {
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        }

        public async Task RunAsync(Func<TcpClient, Task> executeSessionFunc, CancellationToken cancellationToken = default)
        {
            List<TcpClient> connectedClients = new List<TcpClient>();
            TcpListener listener = new TcpListener(Endpoint);

            listener.Start();

            try
            {
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync().WithCancellation(cancellationToken).ConfigureAwait(false);

                    lock (connectedClients) {
                        connectedClients.Add(client);
                    }

                    executeSessionFunc(client)
                        .ContinueWith(_ =>
                        {
                            lock (connectedClients) {
                                connectedClients.Remove(client);
                            }
                        })
                        .AsVoid();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected.
            }
            finally
            {
                // Nuke all connections.
                lock (connectedClients)
                {
                    foreach (TcpClient client in connectedClients) {
                        client.Close();
                    }

                    connectedClients.Clear();
                }

                listener.Stop();
            }
        }
    }
}