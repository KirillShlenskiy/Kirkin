﻿using System;
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
            int targetCount = 100000;
            DataTableLite dt = CreateDataTableLite(targetCount);

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
            int targetCount = 100000;
            DataTable dt = CreateDataTableRegular(targetCount);

            Assert.AreEqual(targetCount, dt.Rows.Count);

            for (int i = 0; i < targetCount; i++)
            {
                Assert.AreEqual(i, dt.Rows[i][0]);
                Assert.AreEqual(i.ToString(), dt.Rows[i][1]);
            }
        }

        [Test]
        public void IterationPerfLite()
        {
            DataTableLite dt = CreateDataTableLite(1000);

            for (int i = 0; i < 10000; i++)
            {
                foreach (DataRowLite row in dt.Rows)
                {
                }
            }
        }

        [Test]
        public void IterationPerfRegular()
        {
            DataTable dt = CreateDataTableRegular(1000);

            for (int i = 0; i < 10000; i++)
            {
                foreach (DataRow row in dt.Rows)
                {
                }
            }
        }

        [Test]
        public void ValueAccessByIndexWithExactTypeLite()
        {
            DataTableLite dt = CreateDataTableLite(1);
            DataRowLite row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = row.GetValue<int>(0);
                value = row.GetValue<string>(1);
            }
        }

        [Test]
        public void ValueAccessByIndexWithExactTypeRegular()
        {
            DataTable dt = CreateDataTableRegular(1);
            DataRow row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = row.Field<int>(0);
                value = row.Field<string>(1);
            }
        }

        [Test]
        public void ValueAccessByNameWithExactTypeLite()
        {
            DataTableLite dt = CreateDataTableLite(1);
            DataRowLite row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = row.GetValue<int>("ID");
                value = row.GetValue<string>("Value");
            }
        }

        [Test]
        public void ValueAccessByNameWithExactTypeRegular()
        {
            DataTable dt = CreateDataTableRegular(1);
            DataRow row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = row.Field<int>("ID");
                value = row.Field<string>("Value");
            }
        }

        [Test]
        public void ValueAccessByIndexBoxedLite()
        {
            DataTableLite dt = CreateDataTableLite(1);
            DataRowLite row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = (int)row[0];
                value = (string)row[1];
            }
        }

        [Test]
        public void ValueAccessByIndexBoxedRegular()
        {
            DataTable dt = CreateDataTableRegular(1);
            DataRow row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = (int)row[0];
                value = (string)row[1];
            }
        }

        [Test]
        public void ValueAccessByNameBoxedLite()
        {
            DataTableLite dt = CreateDataTableLite(1);
            DataRowLite row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = (int)row["ID"];
                value = (string)row["Value"];
            }
        }

        [Test]
        public void ValueAccessByNameBoxedRegular()
        {
            DataTable dt = CreateDataTableRegular(1);
            DataRow row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = (int)row["ID"];
                value = (string)row["Value"];
            }
        }

        private static DataTableLite CreateDataTableLite(int rowCount)
        {
            DataTableLite dt = new DataTableLite();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            for (int i = 0; i < rowCount; i++) {
                dt.Rows.Add(i, i.ToString());
            }

            return dt;
        }

        private static DataTable CreateDataTableRegular(int rowCount)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            for (int i = 0; i < rowCount; i++) {
                dt.Rows.Add(i, i.ToString());
            }

            return dt;
        }
    }
}