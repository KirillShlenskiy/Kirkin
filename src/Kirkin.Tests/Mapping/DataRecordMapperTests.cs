using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
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

            var stubs = new List<PersonStub>();

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT TOP 10 * FROM Person ORDER BY PersonID", cn))
            {
                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    var config = new DataRecordToObjectMapperConfig<PersonStub>(reader);
                    var mapper = Mapper.CreateMapper(config);

                    while (reader.Read())
                    {
                        var person = mapper.Map(reader, new PersonStub());

                        stubs.Add(person);
                    }
                }
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

            var stubs = new List<PersonStub>();

            using (var cn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT TOP 10 NULL AS PersonID, DisplayName FROM Person ORDER BY PersonID", cn))
            {
                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    var mapper = Mapper.CreateMapper(new DataRecordToObjectMapperConfig<PersonStub>(reader));

                    while (reader.Read())
                    {
                        var person = mapper.Map(reader, new PersonStub { personID = 1 });

                        stubs.Add(person);
                    }
                }
            }

            Assert.NotEmpty(stubs);
            Assert.True(stubs.All(s => s.personID == 0), "All personIDs expected to be zero.");

            Debug.Print("Done.");
        }

        sealed class PersonStub
        {
            public int personID { get; set; } // Wrong case on purpose to test defaults.
            public string DisplayName { get; set; }
        }
    }
}