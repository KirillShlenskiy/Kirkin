using System;
using System.Data.SqlClient;
using System.Threading;

namespace Kirkin.Data.SqlClient
{
    public sealed class SqlServerConnectionFactory : IDisposable
    {
        private Lazy<SqlConnection> Connection;
        public string ConnectionString { get; }

        public SqlServerConnectionFactory(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Invalid connection string.");

            ConnectionString = connectionString;
            Connection = new Lazy<SqlConnection>(CreateConnection);
        }

        public SqlConnection GetOpenConnection()
        {
            Lazy<SqlConnection> connection = Connection;

            if (connection == null) {
                throw new ObjectDisposedException(nameof(SqlServerConnectionFactory));
            }

            return connection.Value;
        }

        private SqlConnection CreateConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);

            try
            {
                connection.Open();
            }
            catch
            {
                connection.Dispose();

                throw;
            }

            return connection;
        }

        public void Dispose()
        {
            Lazy<SqlConnection> connection = Interlocked.Exchange(ref Connection, null);

            if (connection != null && connection.IsValueCreated) {
                connection.Value.Dispose();
            }
        }
    }
}