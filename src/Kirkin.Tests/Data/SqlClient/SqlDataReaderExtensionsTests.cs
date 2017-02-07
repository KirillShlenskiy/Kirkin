using System;
using System.Data.SqlClient;
using Kirkin.Data.SqlClient;

using NUnit.Framework;

namespace Kirkin.Tests.Data.SqlClient
{
    public class SqlDataReaderExtensionsTests
    {
        private const string ConnectionString = "Data Source=MENZIES; Initial Catalog=master; Integrated Security=True;";
        private const string SQL = "SELECT ROW_NUMBER() OVER ( ORDER BY id ) FROM sysobjects";
        private const int Iterations = 1000;

        [Test]
        public static void Regular()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                for (int i = 0; i < Iterations; i++)
                {
                    using (SqlCommand command = new SqlCommand(SQL, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        long result;

                        while (reader.Read()) {
                            result = (long)reader[0];
                        }
                    }
                }
            }

            Console.WriteLine("Collection counts:");

            for (int i = 0; i < 3; i++) {
                Console.WriteLine($"Gen {i}: {GC.CollectionCount(i)}.");
            }
        }

        [Test]
        public static void Frugal()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                for (int i = 0; i < Iterations; i++)
                {
                    using (SqlCommand command = new SqlCommand(SQL, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        long result;

                        while (reader.Read()) {
                            result = reader.GetValueOrDefaultAlt<long>(0);
                        }
                    }
                }
            }

            for (int i = 0; i < 3; i++) {
                Console.WriteLine($"Gen {i}: {GC.CollectionCount(i)}.");
            }
        }

        public static class MemoryAnalysis
        {
            public static long AveragedApproximateSize<T>(Func<T> rootGenerator) where T : class
            {
                const int iters = 1000000;

                var items = new object[iters];
                long start = GC.GetTotalMemory(true);

                for (int i = 0; i < items.Length; i++)
                    items[i] = rootGenerator();

                long end = GC.GetTotalMemory(true);
                GC.KeepAlive(items);

                return (long)Math.Round((end - start) / (double)iters);
            }
        }
    }
}