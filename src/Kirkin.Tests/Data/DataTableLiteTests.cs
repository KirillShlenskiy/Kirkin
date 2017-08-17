﻿using System;

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

            Assert.AreEqual(3, dt.Rows.Count);

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
        }
    }
}