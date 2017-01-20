using System;

using Kirkin.Refs;

using NUnit.Framework;

namespace Kirkin.Tests.Refs
{
    public class RefTests
    {
        [Test]
        public void PropertyRef()
        {
            Dummy dummy = new Dummy { ID = 123 };
            ValueRef<int> id = ValueRef.FromAssignableExpression(() => dummy.ID);

            Assert.AreEqual(123, id.Value);

            id.Value = 321;

            Assert.AreEqual(321, dummy.ID);

            IRef weakID = id;

            Assert.AreEqual(321, weakID.Value);

            weakID.Value = 111;

            Assert.AreEqual(111, dummy.ID);
            Assert.Throws<Exception>(() => weakID.Value = "222");
        }

        [Test]
        public void LocalRef()
        {
            int value = 123;
            ValueRef<int> valueRef = ValueRef.FromAssignableExpression(() => value);

            Assert.AreEqual(123, valueRef.Value);

            valueRef.Value = 321;

            Assert.AreEqual(321, value);

            IRef weakID = valueRef;

            Assert.AreEqual(321, weakID.Value);

            weakID.Value = 111;

            Assert.AreEqual(111, value);
            Assert.Throws<Exception>(() => weakID.Value = "222");
        }

        //[Test]
        //public void AdjustStructProperty()
        //{
        //    Dummy dummy = new Dummy();

        //    ValueRef
        //        .Capture(() => dummy.Frame)
        //        .Adjust(v => { v.Size.Width = 123; return v; });

        //    Assert.AreEqual(123, dummy.Size.Width);
        //}

        [Test]
        public void MultilevelStructRef()
        {
            Dummy dummy = new Dummy();

            ValueRef<int> widthRef = ValueRef
                .FromAssignableExpression(() => dummy.Frame)
                .Ref(f => f.Size)
                .Ref(s => s.Width);

            Assert.AreEqual(0, widthRef.Value);

            widthRef.Value = 123;

            Assert.AreEqual(123, dummy.Frame.Size.Width);
            Assert.AreEqual(123, widthRef.Value);
        }

        [Test]
        public void MultilevelStructSwap()
        {
            Dummy dummy = new Dummy();

            ValueRef<int> widthRef = ValueRef
                .FromAssignableExpression(() => dummy.Frame)
                .Ref(f => f.Size)
                .Ref(s => s.Width);

            Assert.AreEqual(0, widthRef.Value);

            dummy.Frame = new Frame { Size = new Size { Width = 123 } };

            Assert.AreEqual(123, widthRef.Value);

            dummy = new Dummy();

            Assert.AreEqual(0, widthRef.Value);

            widthRef.Value = 321;

            Assert.AreEqual(321, dummy.Frame.Size.Width);
            Assert.AreEqual(321, widthRef.Value);
        }

        //[Test]
        //public void Api()
        //{
        //    Dummy dummy = new Dummy();

        //    ValueRef.Capture(() => dummy.Frame.Size.Width).Value = 123;
        //}

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
            public Frame Frame { get; set; }
        }

        struct Frame
        {
            public Position Position { get; set; }
            public Size Size { get; set; }
        }

        struct Position
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        struct Size
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }
}