using System;

using Kirkin.Text;

using Xunit;

namespace Kirkin.Tests.Text
{
    public class UrlUtilTests
    {
        [Fact]
        public void EmptySegments()
        {
            var segment1 = "http://www.google.com.au";
            var segment2 = "q?=stuff";
            var correct = segment1 + "/" + segment2;

            Assert.Equal(correct, UrlUtil.Combine(string.Empty, segment1, segment2));
            Assert.Equal(correct, UrlUtil.Combine(segment1, string.Empty, segment2));
            Assert.Equal(correct, UrlUtil.Combine(segment1, segment2, string.Empty));
        }

        [Fact]
        public void InvalidInput()
        {
            var segment1 = "http://www.google.com.au";
            var segment2 = "q?=stuff";

            Assert.Throws<ArgumentNullException>(() => UrlUtil.Combine(null, null));
            Assert.Throws<ArgumentException>(() => UrlUtil.Combine());
            Assert.Throws<ArgumentException>(() => UrlUtil.Combine(segment1));
            Assert.Throws<ArgumentException>(() => UrlUtil.Combine(segment1 + "//", "//" + segment2)); // Double delimiters prohibited.
        }

        [Fact]
        public void TwoParamOverload()
        {
            var segment1 = "http://www.google.com.au";
            var segment2 = "q?=stuff";
            var correct = segment1 + "/" + segment2;

            Assert.Equal(correct, UrlUtil.Combine(segment1, segment2));
            Assert.Equal(correct, UrlUtil.Combine(segment1 + "/", segment2));
            Assert.Equal(correct, UrlUtil.Combine(segment1, "/" + segment2));
            Assert.Equal(correct, UrlUtil.Combine(segment1 + "/", "/" + segment2));
        }

        [Fact]
        public void ThreeParamOverload()
        {
            var segment1 = "http://www.google.com.au";
            var segment2 = "q?=stuff";
            var segment3 = "test";
            var correct = segment1 + "/" + segment2 + "/" + segment3;

            Assert.Equal(correct, UrlUtil.Combine(segment1, segment2, segment3));
            Assert.Equal(correct, UrlUtil.Combine(segment1 + "/", segment2, segment3));
            Assert.Equal(correct, UrlUtil.Combine(segment1, "/" + segment2, segment3));
            Assert.Equal(correct, UrlUtil.Combine(segment1, segment2 + "/", segment3));
            Assert.Equal(correct, UrlUtil.Combine(segment1, segment2, "/" + segment3));
            Assert.Equal(correct, UrlUtil.Combine(segment1 + "/", segment2 + "/", segment3));
            Assert.Equal(correct, UrlUtil.Combine(segment1, "/" + segment2, "/" + segment3));
            Assert.Equal(correct, UrlUtil.Combine(segment1 + "/", "/" + segment2 + "/", "/" + segment3));
        }

        [Fact]
        public void DoubleSlashInSchemaAllowed()
        {
            Assert.Equal("http://www.google.com", UrlUtil.Combine("http://", "www.google.com"));
        }
    }
}