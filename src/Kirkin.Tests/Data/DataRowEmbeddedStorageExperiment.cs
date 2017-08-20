using Kirkin.Memory;

using NUnit.Framework;

namespace Kirkin.Tests.Data
{
    public class DataRowEmbeddedStorageExperiment
    {
        [Test]
        public unsafe void DataRowStorageTest()
        {
            DataColumn idCol = new IntColumn(0);
            DataRow row = new DataRow(4);

            for (int i = 0; i < 1000000; i++) {
                row[idCol] = 123;
            }

            Assert.AreEqual(123, row[idCol]);
        }

        unsafe sealed class DataRow
        {
            private readonly byte[] bytes;

            public DataRow(int sizeBytes)
            {
                bytes = new byte[sizeBytes];
            }

            public object this[DataColumn column]
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

        abstract unsafe class DataColumn
        {
            protected internal abstract object GetValue(void* row);
            protected internal abstract void SetValue(void* row, object value);
        }

        sealed unsafe class IntColumn : DataColumn
        {
            public readonly int _offset;

            public IntColumn(int offset)
            {
                _offset = offset;
            }

            protected internal override object GetValue(void* row)
            {
                return *((int*)row + _offset);
            }

            protected internal override unsafe void SetValue(void* row, object value)
            {
                *((int*)row + _offset) = (int)value;
            }
        }
    }
}