using System;

using Kirkin.Refs;

using Xunit;

namespace Kirkin.Tests.Refs
{
    public class RefTests
    {
        [Fact]
        public void PropertyRef()
        {
            Dummy dummy = new Dummy { ID = 123 };
            ValueRef<int> id = ValueRef.FromExpression(() => dummy.ID);

            Assert.Equal(123, id.Value);

            id.Value = 321;

            Assert.Equal(321, dummy.ID);

            IRef weakID = id;

            Assert.Equal(321, weakID.Value);

            weakID.Value = 111;

            Assert.Equal(111, dummy.ID);
            Assert.ThrowsAny<Exception>(() => weakID.Value = "222");
        }

        [Fact]
        public void LocalRef()
        {
            int value = 123;
            ValueRef<int> valueRef = ValueRef.FromExpression(() => value);

            Assert.Equal(123, valueRef.Value);

            valueRef.Value = 321;

            Assert.Equal(321, value);

            IRef weakID = valueRef;

            Assert.Equal(321, weakID.Value);

            weakID.Value = 111;

            Assert.Equal(111, value);
            Assert.ThrowsAny<Exception>(() => weakID.Value = "222");
        }

        //[Fact]
        //public void AdjustStructProperty()
        //{
        //    Dummy dummy = new Dummy();

        //    ValueRef
        //        .Capture(() => dummy.Frame)
        //        .Adjust(v => { v.Size.Width = 123; return v; });

        //    Assert.Equal(123, dummy.Size.Width);
        //}

        [Fact]
        public void MultilevelStructRef()
        {
            Dummy dummy = new Dummy();

            ValueRef<int> widthRef = ValueRef
                .FromExpression(() => dummy.Frame)
                .Ref(f => f.Size)
                .Ref(s => s.Width);

            Assert.Equal(0, widthRef.Value);

            widthRef.Value = 123;

            Assert.Equal(123, dummy.Frame.Size.Width);
            Assert.Equal(123, widthRef.Value);
        }

        [Fact]
        public void MultilevelStructSwap()
        {
            Dummy dummy = new Dummy();

            ValueRef<int> widthRef = ValueRef
                .FromExpression(() => dummy.Frame)
                .Ref(f => f.Size)
                .Ref(s => s.Width);

            Assert.Equal(0, widthRef.Value);

            dummy.Frame = new Frame { Size = new Size { Width = 123 } };

            Assert.Equal(123, widthRef.Value);

            dummy = new Dummy();

            Assert.Equal(0, widthRef.Value);

            widthRef.Value = 321;

            Assert.Equal(321, dummy.Frame.Size.Width);
            Assert.Equal(321, widthRef.Value);
        }

        //[Fact]
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