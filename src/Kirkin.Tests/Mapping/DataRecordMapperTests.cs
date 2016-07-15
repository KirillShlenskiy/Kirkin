using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Kirkin.Mapping;
using Kirkin.Mapping.Data;

using Xunit;

namespace Kirkin.Tests.Mapping
{
    public class DataRecordMapperTests
    {
        private static readonly string ConnectionString = new SqlConnectionStringBuilder {
            DataSource = "MENZIES",
            InitialCatalog = "DemoStud2008Daily",
            IntegratedSecurity = true
        }.ToString();

        [Fact]
        public void PersonTest()
        {
            if (!Environment.MachineName.Equals("BABUSHKA", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            PersonStub[] stubs;

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT TOP 10 * FROM Person ORDER BY PersonID", cn))
            {
                stubs = cmd
                    .ExecuteEntities<PersonStub>()
                    .ToArray();
            }

            Assert.NotEmpty(stubs);
            Assert.True(stubs.All(s => !string.IsNullOrEmpty(s.DisplayName)), "All DisplayNames expected to be non-empty.");
            Assert.True(stubs.All(s => s.personID != 0), "All personIDs expected to be non-zero.");

            Debug.Print("Done.");
        }

        [Fact]
        public void SubstituteDBNull()
        {
            if (!Environment.MachineName.Equals("BABUSHKA", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var nonNullableStubs = new List<PersonStub>();
            var nullableStubs = new List<NullablePersonStub>();

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT TOP 10 NULL AS PersonID, DisplayName FROM Person ORDER BY PersonID", cn))
            {
                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    var nonNullableMapper = Mapper.Builder.FromDataReaderOrRecord(reader).ToType<PersonStub>().BuildMapper();
                    var nullableMapper = Mapper.Builder.FromDataReaderOrRecord(reader).ToType<NullablePersonStub>().BuildMapper();
                    
                    while (reader.Read())
                    {
                        var nonNullablePerson = nonNullableMapper.Map(reader, new PersonStub { personID = 1 });

                        nonNullableStubs.Add(nonNullablePerson);

                        var nullablePerson = nullableMapper.Map(reader, new NullablePersonStub { PersonID = 1 });

                        nullableStubs.Add(nullablePerson);
                    }
                }
            }

            Assert.NotEmpty(nonNullableStubs);
            Assert.NotEmpty(nullableStubs);
            Assert.True(nonNullableStubs.All(s => s.personID == 0), "All personIDs expected to be zero.");
            Assert.True(nullableStubs.All(s => !s.PersonID.HasValue), "All PersonIDs expected to be NULL.");

            Debug.Print("Done.");
        }

        [Fact]
        public async Task MapDefaultOfTToNullableT()
        {
            if (!Environment.MachineName.Equals("BABUSHKA", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            NullablePersonStub[] stubs;

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT TOP 10 0 AS PersonID, DisplayName FROM Person ORDER BY PersonID", cn)) {
                stubs = await cmd.ExecuteEntitiesAsync<NullablePersonStub>();
            }

            Assert.NotEmpty(stubs);
            Assert.True(stubs.All(s => s.PersonID.HasValue && s.PersonID.Value == 0), "All PersonIDs expected to be zero.");

            Debug.Print("Done.");
        }

        sealed class PersonStub
        {
            public int personID { get; set; } // Wrong case on purpose to test defaults.
            public string DisplayName { get; set; }
        }

        sealed class NullablePersonStub
        {
            public int? PersonID { get; set; }
            public string DisplayName { get; set; }
        }
    }
}