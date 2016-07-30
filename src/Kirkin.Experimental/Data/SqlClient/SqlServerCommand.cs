//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Kirkin.Data.SqlClient
//{
//    /// <summary>
//    /// Type which abstracts away connection and command management.
//    /// </summary>
//    public sealed class SqlServerCommand
//    {
//        public string ConnectionString
//        {
//            get
//            {
//                return ConnectionFactory.ConnectionString;
//            }
//        }

//        private readonly SqlServerConnectionFactory ConnectionFactory;
//        private readonly bool OwnsConnectionFactory;

//        public SqlServerCommand(string connectionString)
//            : this(new SqlServerConnectionFactory(connectionString))
//        {
//        }

//        public SqlServerCommand(SqlServerConnectionFactory connectionFactory)
//        {
//            if (connectionFactory == null) throw new ArgumentNullException(nameof(connectionFactory));

//            ConnectionFactory = connectionFactory;
//        }

//        private SqlServerCommand(SqlServerConnectionFactory connectionFactory, bool ownsConnectionFactory)
//        {
//            ConnectionFactory = connectionFactory;
//            OwnsConnectionFactory = ownsConnectionFactory;
//        }
//    }
//}