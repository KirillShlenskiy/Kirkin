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

            Assert.True(new DataTableDiff(dt1, dt2).AreSame);
        }

        [Fact]
        public void SimpleTableDiff()
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add("ID", typeof(int));
            dt2.Columns.Add("ID", typeof(int));

            Assert.True(new DataTableDiff(dt1, dt2).AreSame);
        }

        [Fact]
        public void ColumnCountMismatchDiff()
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();

            dt1.Columns.Add("ID", typeof(int));

            Assert.False(new DataTableDiff(dt1, dt2).AreSame);
        }
    }
}