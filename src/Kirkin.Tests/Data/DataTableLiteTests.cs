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
        public void DataRowLite_GetValue()
        {
            DataTableLite dt = new DataTableLite();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            dt.Rows.Add(1, "Blah");

            DataRowLite row = dt.Rows[0];

            Assert.AreEqual(1, row.GetValue<int>(0));
            Assert.AreEqual(1, row.GetValue<int>("id"));
            Assert.AreEqual(1, row.GetValue<int>(dt.Columns["id"]));
            Assert.AreEqual("Blah", row.GetValue<string>(1));
            Assert.AreEqual("Blah", row.GetValue<string>("value"));
            Assert.AreEqual("Blah", row.GetValue<string>(dt.Columns["value"]));

            Assert.AreEqual(1, row.GetValue<int?>(0));
        }

        [Test]
        public void DataRowLite_SetValue()
        {
            DataTableLite dt = new DataTableLite();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            DataRowLite row = dt.Rows.AddNewRow();

            Assert.True(row.IsNull("ID"));
            Assert.AreEqual(DBNull.Value, row["ID"]);

            row.SetValue("ID", 1);

            Assert.False(row.IsNull("ID"));
            Assert.AreEqual(1, row["ID"]);

            row.SetValue(0, 2);

            Assert.AreEqual(2, row["ID"]);

            row.SetValue(dt.Columns["ID"], 3);

            Assert.AreEqual(3, row["ID"]);

            row.SetNull("ID");

            Assert.True(row.IsNull("ID"));
            Assert.AreEqual(DBNull.Value, row["ID"]);
        }

        [Test]
        public void DataRowLite_GetValueNullable()
        {
            DataTableLite dt = new DataTableLite();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            dt.Rows.Add(1, "Blah");

            DataRowLite row = dt.Rows[0];

            Assert.AreEqual(1, row.GetValue<int?>(0));

            row[0] = DBNull.Value;

            Assert.True(row.IsNull(0));
            Assert.Null(row.GetValueOrDefault<int?>(0));
        }

        [Test]
        public void ClearRows()
        {
            DataTableLite dt = CreateDataTableLite(5000);

            Assert.True(dt.Rows.Capacity >= 5000);

            dt.Rows.Clear();

            Assert.AreEqual(0, dt.Rows.Count);
            Assert.AreEqual(16, dt.Rows.Capacity);

            dt.Rows.Add(123, "Zzz"); // Count = 1;

            Assert.AreEqual(123, dt.Rows[0][0]);
            Assert.AreEqual("Zzz", dt.Rows[0][1]);
            Assert.Throws<ArgumentOutOfRangeException>(() => dt.Rows[1]?.ToString());

            for (int i = 0; i < 100; i++) {
                dt.Rows.Add(i, i.ToString());
            }

            Assert.AreEqual(101, dt.Rows.Count);
            Assert.AreEqual(99, dt.Rows[100][0]);
            Assert.AreEqual("99", dt.Rows[100][1]);
        }

        [Test]
        public void RemoveRow()
        {
            DataTableLite dt = CreateDataTableLite(5);

            Assert.AreEqual(5, dt.Rows.Count);
            Assert.AreEqual(0, dt.Rows[0][0]);
            Assert.AreEqual("0", dt.Rows[0][1]);

            dt.Rows.Remove(dt.Rows[0]);

            Assert.AreEqual(4, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0][0]);
            Assert.AreEqual("1", dt.Rows[0][1]);

            dt.Rows.Remove(dt.Rows[1]);

            Assert.AreEqual(3, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0][0]);
            Assert.AreEqual("1", dt.Rows[0][1]);
            Assert.AreEqual(3, dt.Rows[1][0]);
            Assert.AreEqual("3", dt.Rows[1][1]);

            dt.Rows.Remove(dt.Rows[2]);

            Assert.AreEqual(2, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0][0]);
            Assert.AreEqual("1", dt.Rows[0][1]);
            Assert.AreEqual(3, dt.Rows[1][0]);
            Assert.AreEqual("3", dt.Rows[1][1]);

            // And add one.
            DataRowLite row = dt.Rows.AddNewRow();

            row[0] = 5;
            row[1] = "5";

            Assert.AreEqual(3, dt.Rows.Count);
            Assert.AreEqual(1, dt.Rows[0][0]);
            Assert.AreEqual("1", dt.Rows[0][1]);
            Assert.AreEqual(3, dt.Rows[1][0]);
            Assert.AreEqual("3", dt.Rows[1][1]);
            Assert.AreEqual(5, dt.Rows[2][0]);
            Assert.AreEqual("5", dt.Rows[2][1]);
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
        public void ColumnExists()
        {
            DataTableLite table = new DataTableLite();

            Assert.False(table.Columns.Contains("zzz"));
            Assert.False(table.Columns.Contains("ZZZ"));

            table.Columns.Add("zzz", typeof(int));

            Assert.True(table.Columns.Contains("zzz"));
            Assert.True(table.Columns.Contains("ZZZ"));
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
        public void ValueAccessByIndex_WithExactType_Lite()
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
        public void ValueAccessByIndex_WithExactType_Regular()
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
        public void ValueAccessByNameWithExactType_Lite()
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
        public void ValueAccessByNameWithExactType_Regular()
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
        public void ValueAccessByNameWithExactType_16Columns_Lite()
        {
            DataTableLite dt = CreateDataTableLite(1, 14);
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
        public void ValueAccessByNameWithExactType_16Columns_Regular()
        {
            DataTable dt = CreateDataTableRegular(1, 14);
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
        public void ValueAccessByIndex_Boxed_Lite()
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
        public void ValueAccessByIndex_Boxed_Regular()
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
        public void ValueAccessByName_Boxed_Lite()
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
        public void ValueAccessByName_Boxed_CaseMismatch_Lite()
        {
            DataTableLite dt = CreateDataTableLite(1);
            DataRowLite row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = (int)row["id"];
                value = (string)row["value"];
            }
        }

        [Test]
        public void ValueAccessByName_Boxed_Regular()
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

        [Test]
        public void ValueAccessByName_Boxed_CaseMismatch_Regular()
        {
            DataTable dt = CreateDataTableRegular(1);
            DataRow row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = (int)row["id"];
                value = (string)row["value"];
            }
        }

        [Test]
        public void ValueAccessByName_Boxed_16Columns_Lite()
        {
            DataTableLite dt = CreateDataTableLite(1, 14);
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
        public void ValueAccessByName_Boxed_16Columns_CaseMismatch_Lite()
        {
            DataTableLite dt = CreateDataTableLite(1, 14);
            DataRowLite row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = (int)row["id"];
                value = (string)row["value"];
            }
        }

        [Test]
        public void ValueAccessByName_Boxed_16Columns_Regular()
        {
            DataTable dt = CreateDataTableRegular(1, 14);
            DataRow row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = (int)row["ID"];
                value = (string)row["Value"];
            }
        }

        [Test]
        public void ValueAccessByName_Boxed_16Columns_CaseMismatch_Regular()
        {
            DataTable dt = CreateDataTableRegular(1, 14);
            DataRow row = dt.Rows[0];
            int id;
            string value;

            for (int i = 0; i < 1000000; i++)
            {
                id = (int)row["id"];
                value = (string)row["value"];
            }
        }

        [Test]
        public void MemPressureLite()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long bytesStarting = GC.GetTotalMemory(false);

            DataTableLite dt = CreateDataTableLite(100000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long bytesEnding = GC.GetTotalMemory(false);

            Console.WriteLine($"Diff: {bytesEnding - bytesStarting:###,###,##0}.");

            GC.KeepAlive(dt);
        }

        [Test]
        public void MemPressureRegular()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long bytesStarting = GC.GetTotalMemory(false);

            DataTable dt = CreateDataTableRegular(100000);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long bytesEnding = GC.GetTotalMemory(false);

            Console.WriteLine($"Diff: {bytesEnding - bytesStarting:###,###,##0}.");

            GC.KeepAlive(dt);
        }

        [Test]
        public void DeleteRow()
        {
            DataTableLite table = CreateDataTableLite(10);

            Assert.AreEqual(10, table.Rows.Count);

            for (int i = 0; i < table.Rows.Count; i++) {
                Assert.AreEqual(i, table.Rows[i][0]);
            }

            table.Rows.Remove(table.Rows[0]);

            Assert.AreEqual(9, table.Rows.Count);

            for (int i = 0; i < table.Rows.Count; i++) {
                Assert.AreEqual(i + 1, table.Rows[i][0]);
            }

            table.Rows.Remove(table.Rows[4]);

            Assert.AreEqual(8, table.Rows.Count);
            Assert.AreEqual(1, table.Rows[0][0]);
            Assert.AreEqual(2, table.Rows[1][0]);
            Assert.AreEqual(3, table.Rows[2][0]);
            Assert.AreEqual(4, table.Rows[3][0]);
            Assert.AreEqual(6, table.Rows[4][0]);
            Assert.AreEqual(7, table.Rows[5][0]);
            Assert.AreEqual(8, table.Rows[6][0]);
            Assert.AreEqual(9, table.Rows[7][0]);

            table.Rows.Insert(4, 5, "Re-inserted");

            Assert.AreEqual(9, table.Rows.Count);

            for (int i = 0; i < table.Rows.Count; i++) {
                Assert.AreEqual(i + 1, table.Rows[i][0]);
            }

            table.Rows.Insert(9, 10, "Inserted at the end");

            Assert.AreEqual(10, table.Rows.Count);

            for (int i = 0; i < table.Rows.Count; i++) {
                Assert.AreEqual(i + 1, table.Rows[i][0]);
            }

            table.Rows.Add(11, "Added at the end");

            Assert.AreEqual(11, table.Rows.Count);

            for (int i = 0; i < table.Rows.Count; i++) {
                Assert.AreEqual(i + 1, table.Rows[i][0]);
            }
        }

        private static DataTableLite CreateDataTableLite(int rowCount, int additionalColumns = 0)
        {
            DataTableLite dt = new DataTableLite();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            for (int i = 0; i < rowCount; i++) {
                dt.Rows.Add(i, i.ToString());
            }

            for (int i = 0; i < additionalColumns; i++) {
                dt.Columns.Add($"Col{i + 1}", typeof(int));
            }

            return dt;
        }

        private static DataTable CreateDataTableRegular(int rowCount, int additionalColumns = 0)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Value", typeof(string));

            for (int i = 0; i < rowCount; i++) {
                dt.Rows.Add(i, i.ToString());
            }

            for (int i = 0; i < additionalColumns; i++) {
                dt.Columns.Add($"Col{i + 1}", typeof(int));
            }

            return dt;
        }
    }
}