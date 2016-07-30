using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

using Kirkin.Collections.Generic;

namespace Kirkin.Data.SqlClient
{
    internal class SqlServerEngine : IDisposable
    {
        public string ConnectionString { get; }
        private SqlConnection _connection;

        ///// <summary>
        ///// Set this to true if you don't care about ambiguous column names.
        ///// The default value is false.
        ///// </summary>
        //public bool AllowAmbiguousColumnNames { get; set; }

        internal SqlServerEngine(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Invalid connection string.");

            ConnectionString = connectionString;
        }

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

            using (SqlCommand command = new SqlCommand(sql, GetOpenConnection()))
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

        #endregion

        #region Util

        private SqlConnection GetOpenConnection()
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

        protected virtual IEnumerable<SqlParameter> ExtractParameters(object parameters)
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

        #endregion

        #region Cleanup

        public void Dispose()
        {
            if (_connection != null) {
                _connection.Dispose();
            }
        }

        #endregion
    }
}