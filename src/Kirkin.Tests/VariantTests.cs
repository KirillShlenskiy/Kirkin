//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using NUnit.Framework;

//namespace Kirkin.Tests
//{
//    public class VariantTests
//    {
//        [Test]
//        public void Int32()
//        {
//            Variant v = new Variant(123);

//            Assert.Equals(123, v.GetValue<int>());
//            Assert.Throws<InvalidCastException>(() => v.GetValue<long>());
//        }
//    }
//}