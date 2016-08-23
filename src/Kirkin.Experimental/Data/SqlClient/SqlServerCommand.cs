using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

using Kirkin.Collections.Generic;

namespace Kirkin.Data.SqlClient
{
    /// <summary>
    /// Type which abstracts away connection and command management.
    /// </summary>
    public sealed class SqlServerCommand
    {
        #region Fields and properties

        private readonly object ConnectionObj; // SqlServerConnectionFactory or connection string.

        public string ConnectionString
        {
            get
            {
                SqlServerConnectionFactory connectionFactory = ConnectionObj as SqlServerConnectionFactory;

                if (connectionFactory != null) {
                    return connectionFactory.ConnectionString;
                }

                return (string)ConnectionObj;
            }
        }

        #endregion

        #region Constructor

        public SqlServerCommand(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Invalid connection string.");

            ConnectionObj = connectionString;
        }

        public SqlServerCommand(SqlServerConnectionFactory connectionFactory)
        {
            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));

            ConnectionObj = connectionFactory;
        }

        #endregion

        #region ExecuteQuery

        public IEnumerable<Dictionary<string, object>> ExecuteQuery(string sql)
        {
            return ExecuteQuery(sql, Array<SqlParameter>.Empty);
        }

        public IEnumerable<Dictionary<string, object>> ExecuteQuery(string sql, object parameters)
        {
            if (string.IsNullOrEmpty(sql)) throw new ArgumentException("Invalid SQL.");
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            return ExecuteQuery(sql, ExtractParameters(parameters).ToArray());
        }

        public IEnumerable<Dictionary<string, object>> ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(sql)) throw new ArgumentException("Invalid SQL.");
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            using (ConnectionManager manager = new ConnectionManager(ConnectionObj))
            using (SqlCommand command = new SqlCommand(sql, manager.GetOpenConnection()))
            {
                command.Parameters.AddRange(parameters.ToArray());

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) {
                        yield return SqlCommandExtensions.ReaderToDictionary(reader);
                    }
                }
            }
        }

        public Dictionary<string, object> SingleOrDefault(string sql)
        {
            return SingleOrDefault(sql, Array<SqlParameter>.Empty);
        }

        public Dictionary<string, object> SingleOrDefault(string sql, object parameters)
        {
            if (string.IsNullOrEmpty(sql)) throw new ArgumentException("Invalid SQL.");
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            return SingleOrDefault(sql, ExtractParameters(parameters).ToArray());
        }

        public Dictionary<string, object> SingleOrDefault(string sql, params SqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(sql)) throw new ArgumentException("Invalid SQL.");
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            using (ConnectionManager manager = new ConnectionManager(ConnectionObj))
            using (SqlCommand command = new SqlCommand(sql, manager.GetOpenConnection()))
            {
                command.Parameters.AddRange(parameters.ToArray());

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Dictionary<string, object> result = null;

                    if (reader.Read()) {
                        result = SqlCommandExtensions.ReaderToDictionary(reader);
                    }

                    if (reader.Read()) {
                        throw new InvalidOperationException("More than one row in the result set.");
                    }

                    return result;
                }
            }
        }

        #endregion

        #region Plumbing

        private IEnumerable<SqlParameter> ExtractParameters(object parameters)
        {
            foreach (PropertyInfo prop in parameters.GetType().GetProperties())
            {
                object value = prop.GetValue(parameters);

                if (value == null) {
                    value = DBNull.Value;
                }

                yield return new SqlParameter("@" + prop.Name, value);
            }
        }

        struct ConnectionManager : IDisposable
        {
            private readonly SqlServerConnectionFactory ConnectionFactory;
            private readonly bool NeedToDisposeFactory;

            internal ConnectionManager(object connectionObj)
            {
                ConnectionFactory = connectionObj as SqlServerConnectionFactory;

                if (ConnectionFactory == null)
                {
                    string connectionString = (string)connectionObj;

                    ConnectionFactory = new SqlServerConnectionFactory(connectionString);
                    NeedToDisposeFactory = true;
                }
                else
                {
                    NeedToDisposeFactory = false;
                }
            }

            public SqlConnection GetOpenConnection()
            {
                return ConnectionFactory.GetOpenConnection();
            }

            public void Dispose()
            {
                if (NeedToDisposeFactory) {
                    ConnectionFactory.Dispose();
                }
            }
        }

        #endregion
    }
}