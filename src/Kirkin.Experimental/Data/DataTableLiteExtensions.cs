using System;
using System.Data;

namespace Kirkin.Data
{
    internal static class DataTableLiteExtensions
    {
        public static IDataReader CreateDataReader(this DataTableLite table)
        {
            return new DataReader(table);
        }

        sealed class DataReader : IDataReader
        {
            private DataTableLite Table;
            private int Index = -1;

            public object this[int i] => Table.Rows[Index][i];
            public object this[string name] => Table.Rows[Index][name];
            public int Depth => throw new NotImplementedException();
            public bool IsClosed => throw new NotImplementedException();
            public int RecordsAffected => throw new NotImplementedException();
            public int FieldCount => Table.Columns.Count;

            internal DataReader(DataTableLite table)
            {
                Table = table;
            }

            public void Close()
            {
                Table = null;
            }

            public void Dispose()
            {
                Close();
            }

            public bool GetBoolean(int i)
            {
                return Table.Rows[Index].GetValue<bool>(i);
            }

            public byte GetByte(int i)
            {
                return Table.Rows[Index].GetValue<byte>(i);
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public char GetChar(int i)
            {
                return Table.Rows[Index].GetValue<char>(i);
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public IDataReader GetData(int i)
            {
                throw new NotImplementedException();
            }

            public string GetDataTypeName(int i)
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime(int i)
            {
                throw new NotImplementedException();
            }

            public decimal GetDecimal(int i)
            {
                throw new NotImplementedException();
            }

            public double GetDouble(int i)
            {
                throw new NotImplementedException();
            }

            public Type GetFieldType(int i)
            {
                throw new NotImplementedException();
            }

            public float GetFloat(int i)
            {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int i)
            {
                throw new NotImplementedException();
            }

            public short GetInt16(int i)
            {
                throw new NotImplementedException();
            }

            public int GetInt32(int i)
            {
                throw new NotImplementedException();
            }

            public long GetInt64(int i)
            {
                return Table.Rows[Index].GetValue<long>(i);
            }

            public string GetName(int i)
            {
                return Table.Columns[i].ColumnName;
            }

            public int GetOrdinal(string name)
            {
                return Table.Columns.IndexOf(Table.Columns[name]);
            }

            public DataTable GetSchemaTable()
            {
                throw new NotImplementedException();
            }

            public string GetString(int i)
            {
                return Table.Rows[Index].GetValue<string>(i);
            }

            public object GetValue(int i)
            {
                return Table.Rows[Index][i];
            }

            public int GetValues(object[] values)
            {
                throw new NotImplementedException();
            }

            public bool IsDBNull(int i)
            {
                return Table.Rows[Index].IsNull(i);
            }

            public bool NextResult()
            {
                return false;
            }

            public bool Read()
            {
                return ++Index < Table.Rows.Count;
            }
        }
    }
}