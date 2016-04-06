using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kirkin.Diff;
using Kirkin.Diff.Data;
using Kirkin.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Kirkin.Tests.Diff
{
    public class DataDiffTests
    {
        private readonly Logger Output;

        public DataDiffTests(ITestOutputHelper output)
        {
            Output = Logger
                .Create(output.WriteLine)
                .WithFormatters(EntryFormatter.TimestampNonEmptyEntries());
        }

        [Fact]
        public void EmptyTableDiff()
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            Assert.True(DataTableDiff.Compare(dt1, dt2).AreSame);
        }

        [Fact]
        public void SimpleTableDiff()
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add("ID", typeof(int));
            dt2.Columns.Add("ID", typeof(int));

            Assert.True(DataTableDiff.Compare(dt1, dt2).AreSame);
        }

        [Fact]
        public void ColumnCountMismatchDiff()
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add("ID", typeof(int));

            Assert.False(DataTableDiff.Compare(dt1, dt2).AreSame);
        }

        [Fact(Skip = "Fix")]
        public void ColumnNameMismatchDiff()
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add("ID", typeof(int));
            dt2.Columns.Add("IDz", typeof(int));

            Assert.False(DataTableDiff.Compare(dt1, dt2).AreSame);
        }

        [Fact]
        public void DataCompare()
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add("ID", typeof(int));
            dt2.Columns.Add("ID", typeof(int));
            dt1.Columns.Add("Value", typeof(string));
            dt2.Columns.Add("Value", typeof(string));

            dt1.Rows.Add(1, "Hello");
            dt2.Rows.Add(1, "Hello");
            dt1.Rows.Add(2, "Moshi Moshi");
            dt2.Rows.Add(2, "Moshi Moshi");

            Assert.True(DataTableDiff.Compare(dt1, dt2).AreSame);

            dt1.Rows.Add(3, "Aloha");

            DiffResult diff1 = DataTableDiff.Compare(dt1, dt2);

            Assert.False(diff1.AreSame);
            Assert.Equal("DataTable -> Row count: 3 vs 2.", diff1.ToString());

            dt2.Rows.Add(4, "Whaaaa");

            DiffResult diff2 = DataTableDiff.Compare(dt1, dt2);

            Assert.False(diff2.AreSame);
            //Assert.Equal("Row count mismatch: 3 vs 2.", diff.Message);

            Output.Log(diff2.Message);
        }
    }
}