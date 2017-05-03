using Kirkin.ChangeTracking;
using Kirkin.Serialization;

using NUnit.Framework;

namespace Kirkin.Tests.Serialization
{
    public class XmlSerializerTests
    {
        [Test]
        public void BasicSerialization()
        {
            XmlSerializer serializer = new XmlSerializer();
            Dummy original = new Dummy { ID = 123, Value = "Test" };
            string serialized = serializer.Serialize(original);

            Assert.AreEqual(
                @"<?xml version=""1.0"" encoding=""utf-8""?>
<Root>
  <ID>123</ID>
  <Value>Test</Value>
</Root>", serialized);

            Dummy clone = serializer.Deserialize<Dummy>(serialized);

            Assert.True(PropertyValueEqualityComparer<Dummy>.Default.Equals(original, clone));
            Assert.AreNotSame(original, clone);
        }

        public class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}