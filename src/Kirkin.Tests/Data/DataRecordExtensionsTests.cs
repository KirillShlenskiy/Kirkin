﻿using System;
using System.Data.SqlClient;

using Kirkin.Data;

using NUnit.Framework;

namespace Kirkin.Tests.Data
{
    public class DataRecordExtensionsTests
    {
        private const string SqlRowNum = "SELECT ROW_NUMBER() OVER ( ORDER BY id ) FROM sysobjects";
        private const string SqlNull = "SELECT NULL FROM sysobjects";
        private const int Iterations = 1000;
        private readonly string ConnectionString;

        public DataRecordExtensionsTests()
        {
            if (Environment.MachineName.Equals("BABUSHKA", StringComparison.OrdinalIgnoreCase) ||
                Environment.MachineName.Equals("KIRKINPUTER", StringComparison.OrdinalIgnoreCase))
            {
                ConnectionString = @"Data Source=.; Initial Catalog=master; Integrated Security=True;";
            }
            else
            {
                Assert.Ignore("No connection string defined.");
            }
        }

        [Test]
        [Order(1)]
        public void Regular()
        {
            GCCollectionCounter counter = GCCollectionCounter.StartNew();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                for (int i = 0; i < Iterations; i++)
                {
                    using (SqlCommand command = new SqlCommand(SqlRowNum, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        long result;

                        while (reader.Read()) {
                            result = (long)reader[0];
                        }
                    }
                }
            }

            counter.PrintCounts();
        }

        [Test]
        [Order(2)]
        public void Optimized()
        {
            GCCollectionCounter counter = GCCollectionCounter.StartNew();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                for (int i = 0; i < Iterations; i++)
                {
                    using (SqlCommand command = new SqlCommand(SqlRowNum, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        long result;

                        while (reader.Read()) {
                            result = reader.GetValueOrDefault<long>(0);
                        }
                    }
                }
            }

            counter.PrintCounts();
        }

        [Test]
        [Ignore("")]
        public void DbNullBenchmarkRegular()
        {
            GCCollectionCounter counter = GCCollectionCounter.StartNew();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                for (int i = 0; i < Iterations; i++)
                {
                    using (SqlCommand command = new SqlCommand(SqlRowNum, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        bool result;

                        while (reader.Read()) {
                            result = reader[0] is DBNull;
                        }
                    }
                }
            }

            counter.PrintCounts();
        }

        [Test]
        [Ignore("")]
        public void DbNullBenchmarkIsNull()
        {
            GCCollectionCounter counter = GCCollectionCounter.StartNew();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                for (int i = 0; i < Iterations; i++)
                {
                    using (SqlCommand command = new SqlCommand(SqlRowNum, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        bool result;

                        while (reader.Read()) {
                            result = reader.IsDBNull(0);
                        }
                    }
                }
            }

            counter.PrintCounts();
        }

        [Test]
        [Ignore("")]
        public void DbNullBenchmarkOptimized()
        {
            GCCollectionCounter counter = GCCollectionCounter.StartNew();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                for (int i = 0; i < Iterations; i++)
                {
                    using (SqlCommand command = new SqlCommand(SqlRowNum, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        bool result;

                        while (reader.Read()) {
                            result = reader.GetValueOrDefault<long>(0) == 0;
                        }
                    }
                }
            }

            counter.PrintCounts();
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
                    GC.CollectionCount(2),
                    GC.GetTotalMemory(false)
                );
            }

            private int Gen0;
            private int Gen1;
            private int Gen2;
            private long StartMemory;

            private GCCollectionCounter(int gen0, int gen1, int gen2, long startMemory)
            {
                Gen0 = gen0;
                Gen1 = gen1;
                Gen2 = gen2;
                StartMemory = startMemory;
            }

            public int CollectionCount(int generation)
            {
                if (generation == 0) return GC.CollectionCount(0) - Gen0;
                if (generation == 1) return GC.CollectionCount(1) - Gen1;
                if (generation == 2) return GC.CollectionCount(2) - Gen2;

                throw new ArgumentOutOfRangeException(nameof(generation));
            }

            public long MemoryDiff()
            {
                return GC.GetTotalMemory(false) - StartMemory;
            }

            public void PrintCounts()
            {


                Console.WriteLine("Collection counts:");

                for (int i = 0; i < 3; i++) {
                    Console.WriteLine($"Gen {i}: {CollectionCount(i)}.");
                }

                Console.WriteLine($"Diff: {MemoryDiff()}.");
            }
        }
    }
}