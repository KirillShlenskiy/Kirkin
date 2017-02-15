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
        // True if the underlying connection object
        // is meant to be disposed by this instance.
        private readonly bool OwnsConnection;

        // SqlConnection or string (connection string) or null (when disposed).
        private object _connectionObj;

        /// <summary>
        /// Gets the connection string of the connection managed by this instance.
        /// </summary>
        internal string ConnectionString
        {
            get
            {
                ThrowIfDisposed();

                return _connectionObj as string ?? (_connectionObj as SqlConnection)?.ConnectionString;
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

                SqlConnection connection = (_connectionObj as SqlConnection) ?? CreateConnection();

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

        public SqlServerConnectionManager(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            _connectionObj = connection;
            OwnsConnection = false;
        }

        public SqlServerConnectionManager(Func<SqlConnection> connectionFactory)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));

            _connectionObj = connectionFactory;
            OwnsConnection = true;
        }

        private SqlConnection CreateConnection()
        {
            object connectionObj = _connectionObj;
            SqlConnection connection = connectionObj as SqlConnection;

            if (connection == null)
            {
                Func<SqlConnection> factory = connectionObj as Func<SqlConnection>;

                if (factory != null)
                {
                    connection = factory();
                }
                else
                {
                    string connectionString = (string)connectionObj;

                    connection = new SqlConnection(connectionString);
                }

                connectionObj = Interlocked.CompareExchange(ref _connectionObj, connection, null);

                if (connectionObj != null)
                {
                    // Someone raced us and won.
                    connection.Dispose();

                    connection = (SqlConnection)connectionObj;
                }
            }

            return connection;
        }

        /// <summary>
        /// Release all resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            object connectionObj = Interlocked.Exchange(ref _connectionObj, null);

            if (connectionObj != null && OwnsConnection) {
                ((SqlConnection)connectionObj).Dispose();
            }
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if this instance is marked as disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (_connectionObj == null) {
                throw new ObjectDisposedException(nameof(SqlServerConnectionManager));
            }
        }
    }
}