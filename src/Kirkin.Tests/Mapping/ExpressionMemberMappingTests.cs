using System.Collections.Generic;

using Kirkin.Mapping;

using Xunit;

namespace Kirkin.Tests.Mapping
{
    public class ExpressionMemberMappingTests
    {
        [Fact]
        public void Getter()
        {
            Member<Dictionary<string, object>> idMember = new ExpressionMember<Dictionary<string, object>, int>("ID", dict => (int)dict["ID"]);
            Member<Dictionary<string, object>> valueMember = new ExpressionMember<Dictionary<string, object>, string>("Value", dict => (string)dict["Value"]);

            Mapper<Dictionary<string, object>, Dummy> mapper = Mapper.Builder
                .From(new[] { idMember, valueMember })
                .To<Dummy>()
                .BuildMapper();

            Dictionary<string, object> values = new Dictionary<string, object> {
                { "ID", 123 },
                { "Value", "Test" }
            };

            Dummy d = mapper.Map(values);

            Assert.Equal(123, d.ID);
            Assert.Equal("Test", d.Value);
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}