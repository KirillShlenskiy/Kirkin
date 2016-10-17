using System.Collections.Generic;

using Kirkin.Mapping;

using Xunit;

namespace Kirkin.Tests.Mapping
{
    public class DelegateMemberMappingTests
    {
        [Fact]
        public void Getter()
        {
            // Getter only.
            Member<Dictionary<string, object>> idMember = DelegateMember.ReadOnly<Dictionary<string, object>, int>("ID", dict => (int)dict["ID"]);

            // Getter and setter.
            Member<Dictionary<string, object>> valueMember = DelegateMember.ReadWrite<Dictionary<string, object>, string>(
                "Value", dict => (string)dict["Value"], (dict, value) => dict["Value"] = value
            );

            Mapper<Dictionary<string, object>, Dummy> mapper = Mapper.Builder
                .FromMembers(new[] { idMember, valueMember })
                .ToPublicInstanceProperties<Dummy>()
                .BuildMapper();

            Dictionary<string, object> values = new Dictionary<string, object> {
                { "ID", 123 },
                { "Value", "Test" }
            };

            Dummy d = mapper.Map(values);

            Assert.Equal(123, d.ID);
            Assert.Equal("Test", d.Value);
        }

        [Fact]
        public void Setter()
        {
            // Setter only.
            Member<Dictionary<string, object>> idMember = DelegateMember.WriteOnly<Dictionary<string, object>, int>("ID", (dict, id) => dict["ID"] = id);

            // Getter and setter.
            Member<Dictionary<string, object>> valueMember = DelegateMember.ReadWrite<Dictionary<string, object>, string>(
                "Value", dict => (string)dict["Value"], (dict, value) => dict["Value"] = value
            );

            Mapper<Dummy, Dictionary<string, object>> mapper = Mapper.Builder
                .FromPublicInstanceProperties<Dummy>()
                .ToMembers(new[] { idMember, valueMember })
                .BuildMapper();

            Dummy d = new Dummy {
                ID = 123,
                Value = "Test"
            };

            // Default Dictionary constructor will be used.
            Dictionary<string, object> values = mapper.Map(d);

            Assert.Equal(2, values.Count);
            Assert.Equal(123, values["ID"]);
            Assert.Equal("Test", values["Value"]);

            d.ID = 321;
            d.Value = "tseT";

            mapper.Map(d, values);

            Assert.Equal(2, values.Count);
            Assert.Equal(321, values["ID"]);
            Assert.Equal("tseT", values["Value"]);
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}