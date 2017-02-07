using System;
using System.Data.SqlClient;

using Kirkin.Data.SqlClient;

using NUnit.Framework;

namespace Kirkin.Tests.Data.SqlClient
{
    public class SqlDataReaderExtensionsTests
    {
        private const string ConnectionString = "Data Source=.; Initial Catalog=master; Integrated Security=True;";
        private const string SQL = "SELECT ROW_NUMBER() OVER ( ORDER BY id ) FROM sysobjects";
        private const int Iterations = 2000;

        [Test]
        public static void Regular()
        {
            GCCollectionCounter counter = GCCollectionCounter.StartNew();

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
                Console.WriteLine($"Gen {i}: {counter.CollectionCount(i)}.");
            }
        }

        [Test]
        public static void Frugal()
        {
            GCCollectionCounter counter = GCCollectionCounter.StartNew();

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
                            result = reader.GetValueOrDefault<long>(0);
                        }
                    }
                }
            }

            Console.WriteLine("Collection counts:");

            for (int i = 0; i < 3; i++) {
                Console.WriteLine($"Gen {i}: {counter.CollectionCount(i)}.");
            }
        }

        private sealed class GCCollectionCounter
        {
            public static GCCollectionCounter StartNew()
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                return new GCCollectionCounter(
                    GC.CollectionCount(0),
                    GC.CollectionCount(1),
                    GC.CollectionCount(2)
                );
            }

            private int Gen0;
            private int Gen1;
            private int Gen2;

            private GCCollectionCounter(int gen0, int gen1, int gen2)
            {
                Gen0 = gen0;
                Gen1 = gen1;
                Gen2 = gen2;
            }

            public int CollectionCount(int generation)
            {
                if (generation == 0) return GC.CollectionCount(0) - Gen0;
                if (generation == 1) return GC.CollectionCount(1) - Gen1;
                if (generation == 2) return GC.CollectionCount(2) - Gen2;

                throw new ArgumentOutOfRangeException(nameof(generation));
            }
        }
    }
}