using System.Collections.Generic;

using Kirkin.Mapping;

using Xunit;

namespace Kirkin.Tests.Mapping
{
    public class ExpressionMemberMappingTests
    {
        [Fact]
        public void GenericGetter()
        {
            Member<Dictionary<string, object>> idMember = new ExpressionMember<Dictionary<string, object>, int>("ID", dict => (int)dict["ID"]);
            Member<Dictionary<string, object>> valueMember = new ExpressionMember<Dictionary<string, object>, string>("Value", dict => (string)dict["Value"]);

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
        public void NonGenericGetter()
        {
            Member<Dictionary<string, object>> idMember = new ExpressionMember<Dictionary<string, object>>("ID", dict => (int)dict["ID"]);
            Member<Dictionary<string, object>> valueMember = new ExpressionMember<Dictionary<string, object>>("Value", dict => (string)dict["Value"]);

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
        public void DelegateMappingBenchmark()
        {
            Member<Dictionary<string, object>> idMember = DelegateMember.ReadOnly<Dictionary<string, object>, int>("ID", dict => (int)dict["ID"]);
            Member<Dictionary<string, object>> valueMember = DelegateMember.ReadOnly<Dictionary<string, object>, string>("Value", dict => (string)dict["Value"]);

            Mapper<Dictionary<string, object>, Dummy> mapper = Mapper.Builder
                .FromMembers(new[] { idMember, valueMember })
                .ToPublicInstanceProperties<Dummy>()
                .BuildMapper();

            Dictionary<string, object> values = new Dictionary<string, object> {
                { "ID", 123 },
                { "Value", "Test" }
            };

            Dummy d = new Dummy();

            for (int i = 0; i < 5000000; i++) {
                mapper.Map(values, d);
            }
        }

        [Fact]
        public void GenericExpressionMappingBenchmark()
        {
            Member<Dictionary<string, object>> idMember = new ExpressionMember<Dictionary<string, object>, int>("ID", dict => (int)dict["ID"]);
            Member<Dictionary<string, object>> valueMember = new ExpressionMember<Dictionary<string, object>, string>("Value", dict => (string)dict["Value"]);

            Mapper<Dictionary<string, object>, Dummy> mapper = Mapper.Builder
                .FromMembers(new[] { idMember, valueMember })
                .ToPublicInstanceProperties<Dummy>()
                .BuildMapper();

            Dictionary<string, object> values = new Dictionary<string, object> {
                { "ID", 123 },
                { "Value", "Test" }
            };

            Dummy d = new Dummy();

            for (int i = 0; i < 5000000; i++) {
                mapper.Map(values, d);
            }
        }

        [Fact]
        public void NonGenericExpressionMappingBenchmark()
        {
            Member<Dictionary<string, object>> idMember = new ExpressionMember<Dictionary<string, object>>("ID", dict => (int)dict["ID"]);
            Member<Dictionary<string, object>> valueMember = new ExpressionMember<Dictionary<string, object>>("Value", dict => (string)dict["Value"]);

            Mapper<Dictionary<string, object>, Dummy> mapper = Mapper.Builder
                .FromMembers(new[] { idMember, valueMember })
                .ToPublicInstanceProperties<Dummy>()
                .BuildMapper();

            Dictionary<string, object> values = new Dictionary<string, object> {
                { "ID", 123 },
                { "Value", "Test" }
            };

            Dummy d = new Dummy();

            for (int i = 0; i < 5000000; i++) {
                mapper.Map(values, d);
            }
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}