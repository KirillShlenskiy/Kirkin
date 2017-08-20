using System;
using System.Data;

using Kirkin.Data;
using Kirkin.Memory;

using NUnit.Framework;

namespace Kirkin.Tests.Data
{
    public class DataRowEmbeddedStorageExperiment
    {
        [Test]
        public unsafe void DataRowEmbeddedStorageTest()
        {
            EmbeddedDataColumn idCol = new EmbeddedIntColumn(0);
            EmbeddedDataColumn valueCol = new EmbeddedIntColumn(4);
            EnbeddedDataRow row = new EnbeddedDataRow(8);

            for (int i = 0; i < 1000000; i++)
            {
                row[idCol] = 123;
                row[valueCol] = 321;
            }

            Assert.AreEqual(123, row[idCol]);
            Assert.AreEqual(321, row[valueCol]);
        }

        [Test]
        public void DataRowStandardTest()
        {
            DataTable dt = new DataTable();
            DataColumn idCol = dt.Columns.Add("ID", typeof(int));
            DataColumn valueCol = dt.Columns.Add("Value", typeof(int));
            DataRow row = dt.NewRow();

            for (int i = 0; i < 1000000; i++)
            {
                row[idCol] = 123;
                row[valueCol] = 321;
            }

            Assert.AreEqual(123, row[idCol]);
            Assert.AreEqual(321, row[valueCol]);
        }

        [Test]
        public void DataRowLiteTest()
        {
            DataTableLite dt = new DataTableLite();
            DataColumnLite idCol = dt.Columns.Add("ID", typeof(int));
            DataColumnLite valueCol = dt.Columns.Add("Value", typeof(int));
            DataRowLite row = dt.Rows.AddNewRow();

            for (int i = 0; i < 1000000; i++)
            {
                row[idCol] = 123;
                row[valueCol] = 321;
            }

            Assert.AreEqual(123, row[idCol]);
            Assert.AreEqual(321, row[valueCol]);
        }

        unsafe sealed class EnbeddedDataRow
        {
            private readonly byte[] bytes;

            public EnbeddedDataRow(int sizeBytes)
            {
                bytes = new byte[sizeBytes];
            }

            public object this[EmbeddedDataColumn column]
            {
                get
                {
                    using (Pinned r = new Pinned(bytes)) {
                        return column.GetValue(r.Pointer);
                    }
                }
                set
                {
                    using (Pinned r = new Pinned(bytes)) {
                        column.SetValue(r.Pointer, value);
                    }
                }
            }
        }

        abstract unsafe class EmbeddedDataColumn
        {
            protected internal abstract object GetValue(void* row);
            protected internal abstract void SetValue(void* row, object value);
        }

        sealed unsafe class EmbeddedIntColumn : EmbeddedDataColumn
        {
            public readonly int offset_i32;

            public EmbeddedIntColumn(int offset)
            {
                if (offset % 4 != 0) throw new ArgumentException("Expecting offset to be divisible by 4.");

                offset_i32 = offset / 4;
            }

            protected internal override object GetValue(void* row)
            {
                return *((int*)row + offset_i32);
            }

            protected internal override unsafe void SetValue(void* row, object value)
            {
                *((int*)row + offset_i32) = (int)value;
            }
        }
    }
}