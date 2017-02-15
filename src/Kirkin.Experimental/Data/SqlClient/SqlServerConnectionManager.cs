using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace Kirkin.Data.SqlClient
{
    /// <summary>
    /// Lazy <see cref="SqlConnection"/> lifetime manager.
    /// </summary>
    public sealed class SqlServerConnectionManager : IDisposable
    {
        // SqlConnection or string (connection string) or null (when disposed).
        private object _connectionObj;
        private bool OwnsConnection;

        /// <summary>
        /// Gets the connection string of the connection managed by this instance.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                ThrowIfDisposed();

                return _connectionObj as string ?? ((SqlConnection)_connectionObj).ConnectionString;
            }
        }

        /// <summary>
        /// Gets the open, ready-to-use <see cref="SqlConnection"/> managed by this instance.
        /// </summary>
        public SqlConnection Connection
        {
            get
            {
                ThrowIfDisposed();

                object connectionObj = _connectionObj;
                SqlConnection connection = connectionObj as SqlConnection;

                if (connection == null)
                {
                    string connectionString = (string)connectionObj;

                    connection = new SqlConnection(connectionString);

                    if (Interlocked.CompareExchange(ref _connectionObj, connection, null) != null) {
                        connection = (SqlConnection)_connectionObj;
                    }
                }

                if (connection.State != ConnectionState.Open) {
                    connection.Open();
                }

                return connection;
            }
        }

        public SqlServerConnectionManager(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Invalid connection string.");

            _connectionObj = connectionString;
            OwnsConnection = true;
        }

        public SqlServerConnectionManager(SqlConnection connection, bool ownsConnection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _connectionObj = connection;
            OwnsConnection = ownsConnection;
        }

        public void Dispose()
        {
            object connectionObj = Interlocked.Exchange(ref _connectionObj, null);

            if (connectionObj != null && OwnsConnection) {
                ((SqlConnection)connectionObj).Dispose();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_connectionObj == null) {
                throw new ObjectDisposedException(nameof(SqlServerConnectionManager));
            }
        }
    }
}