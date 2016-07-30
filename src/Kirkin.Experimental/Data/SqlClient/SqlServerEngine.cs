using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Kirkin.Data.SqlClient
{
    internal sealed class SqlServerEngine : IDisposable
    {
        public string ConnectionString { get; }
        private SqlConnection _connection;

        private SqlConnection Connection
        {
            get
            {
                if (_connection == null)
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

                    _connection = connection;
                }

                return _connection;
            }
        }

        internal SqlServerEngine(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Invalid connection string.");

            ConnectionString = connectionString;
        }

        public IEnumerable<Dictionary<string, object>> ExecuteQuery(string sql)
        {
            return ExecuteQuery(sql, Enumerable.Empty<SqlParameter>());
        }

        public IEnumerable<Dictionary<string, object>> ExecuteQuery(string sql, object parameters)
        {
            if (string.IsNullOrEmpty(sql)) throw new ArgumentException("Invalid SQL.");
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            foreach (PropertyInfo prop in parameters.GetType().GetProperties())
            {
                object value = prop.GetValue(parameters);

                if (value == null) {
                    value = DBNull.Value;
                }

                sqlParameters.Add(new SqlParameter("@" + prop.Name, value));
            }

            return ExecuteQuery(sql, sqlParameters.ToArray());
        }

        public IEnumerable<Dictionary<string, object>> ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            if (string.IsNullOrEmpty(sql)) throw new ArgumentException("Invalid SQL.");
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));

            using (SqlCommand command = new SqlCommand(sql, Connection))
            {
                command.Parameters.AddRange(parameters.ToArray());

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read()) {
                        yield return ReaderToDictionary(reader);
                    }
                }
            }
        }

        private static Dictionary<string, object> ReaderToDictionary(SqlDataReader reader)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string name = reader.GetName(i);
                object value = reader[i];

                if (value == DBNull.Value) {
                    value = null;
                }

                dict.Add(name, value);
            }

            return dict;
        }

        public void Dispose()
        {
            if (_connection != null) {
                _connection.Dispose();
            }
        }
    }
}