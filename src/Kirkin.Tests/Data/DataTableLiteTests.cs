using System;
using System.Data;

using Kirkin.Data;

using NUnit.Framework;

namespace Kirkin.Tests.Data
{
    public class DataTableLiteTests
    {
        [Test]
        public void BasicApi()
        {
            DataTableLite dt = new DataTableLite();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            dt.Rows.Add(1, "Blah");
            dt.Rows.Add(2, "Zzz");
            dt.Rows.Add(3, DBNull.Value);
            dt.Rows.AddNewRow();

            Assert.AreEqual(4, dt.Rows.Count);

            Assert.AreEqual(1, dt.Rows[0][0]);
            Assert.AreEqual("Blah", dt.Rows[0][1]);
            Assert.AreEqual(1, dt.Rows[0]["id"]);
            Assert.AreEqual("Blah", dt.Rows[0]["value"]);

            Assert.AreEqual(2, dt.Rows[1][0]);
            Assert.AreEqual("Zzz", dt.Rows[1][1]);
            Assert.AreEqual(2, dt.Rows[1]["id"]);
            Assert.AreEqual("Zzz", dt.Rows[1]["value"]);

            Assert.AreEqual(3, dt.Rows[2][0]);
            Assert.AreEqual(DBNull.Value, dt.Rows[2][1]);
            Assert.AreEqual(3, dt.Rows[2]["id"]);
            Assert.AreEqual(DBNull.Value, dt.Rows[2]["value"]);

            Assert.True(dt.Rows[3].IsNull(0));
            Assert.True(dt.Rows[3].IsNull(1));
            Assert.AreEqual(DBNull.Value, dt.Rows[3][0]);
            Assert.AreEqual(DBNull.Value, dt.Rows[3][1]);
        }

        [Test]
        public void GrowthLite()
        {
            DataTableLite dt = new DataTableLite();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            int targetCount = 100000;

            for (int i = 0; i < targetCount; i++) {
                dt.Rows.Add(i, i.ToString());
            }

            Assert.AreEqual(targetCount, dt.Rows.Count);

            for (int i = 0; i < targetCount; i++)
            {
                Assert.AreEqual(i, dt.Rows[i][0]);
                Assert.AreEqual(i.ToString(), dt.Rows[i][1]);
            }
        }

        [Test]
        public void GrowthRegular()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            int targetCount = 100000;

            for (int i = 0; i < targetCount; i++) {
                dt.Rows.Add(i, i.ToString());
            }

            Assert.AreEqual(targetCount, dt.Rows.Count);

            for (int i = 0; i < targetCount; i++)
            {
                Assert.AreEqual(i, dt.Rows[i][0]);
                Assert.AreEqual(i.ToString(), dt.Rows[i][1]);
            }
        }
    }
}