using System;

using Kirkin.Text;

using NUnit.Framework;

namespace Kirkin.Tests.Text
{
    public class UrlUtilTests
    {
        [Test]
        public void EmptySegments()
        {
            var segment1 = "http://www.google.com.au";
            var segment2 = "q?=stuff";
            var correct = segment1 + "/" + segment2;

            Assert.AreEqual(correct, UrlUtil.Combine(string.Empty, segment1, segment2));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1, string.Empty, segment2));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1, segment2, string.Empty));
        }

        [Test]
        public void InvalidInput()
        {
            var segment1 = "http://www.google.com.au";
            var segment2 = "q?=stuff";

            Assert.Throws<ArgumentNullException>(() => UrlUtil.Combine(null, null));
            Assert.Throws<ArgumentException>(() => UrlUtil.Combine());
            Assert.Throws<ArgumentException>(() => UrlUtil.Combine(segment1));
            Assert.Throws<ArgumentException>(() => UrlUtil.Combine(segment1 + "//", "//" + segment2)); // Double delimiters prohibited.
        }

        [Test]
        public void TwoParamOverload()
        {
            var segment1 = "http://www.google.com.au";
            var segment2 = "q?=stuff";
            var correct = segment1 + "/" + segment2;

            Assert.AreEqual(correct, UrlUtil.Combine(segment1, segment2));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1 + "/", segment2));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1, "/" + segment2));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1 + "/", "/" + segment2));
        }

        [Test]
        public void ThreeParamOverload()
        {
            var segment1 = "http://www.google.com.au";
            var segment2 = "q?=stuff";
            var segment3 = "test";
            var correct = segment1 + "/" + segment2 + "/" + segment3;

            Assert.AreEqual(correct, UrlUtil.Combine(segment1, segment2, segment3));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1 + "/", segment2, segment3));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1, "/" + segment2, segment3));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1, segment2 + "/", segment3));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1, segment2, "/" + segment3));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1 + "/", segment2 + "/", segment3));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1, "/" + segment2, "/" + segment3));
            Assert.AreEqual(correct, UrlUtil.Combine(segment1 + "/", "/" + segment2 + "/", "/" + segment3));
        }

        [Test]
        public void DoubleSlashInSchemaAllowed()
        {
            Assert.AreEqual("http://www.google.com", UrlUtil.Combine("http://", "www.google.com"));
        }
    }
}