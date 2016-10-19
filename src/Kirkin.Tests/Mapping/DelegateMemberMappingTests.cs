﻿using System.Collections.Generic;
using System.Linq;

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

        [Fact]
        public void SimulateTypeMappingMapAndCountChanges()
        {
            int changeCount = 0;

            Mapper<Dummy, Dummy> mapper = Mapper.Builder
                .FromPublicInstanceProperties<Dummy>()
                .ToMembers(
                    PropertyMember
                        .PublicInstanceProperties<Dummy>()
                        .Select(m =>
                        {
                            Member<Dummy> changeTrackingMemberDecorator = DelegateMember.ReadWrite<Dummy, object>(
                                m.Name,
                                d => m.Property.GetValue(d),
                                (d, v) =>
                                {
                                    object currentValue = m.Property.GetValue(d);

                                    if (!Equals(v, currentValue))
                                    {
                                        m.Property.SetValue(d, v);

                                        changeCount++;
                                    }
                                }
                            );

                            return changeTrackingMemberDecorator;
                        })
                        .ToArray()
                )
                .BuildMapper();

            Dummy dummy1 = new Dummy();
            Dummy dummy2 = new Dummy();

            mapper.Map(dummy1, dummy2);

            Assert.Equal(0, changeCount);

            dummy1.ID = 123;
            dummy1.Value = "Zzz";

            mapper.Map(dummy1, dummy2);

            Assert.Equal(123, dummy2.ID);
            Assert.Equal("Zzz", dummy2.Value);
            Assert.Equal(2, changeCount);
        }
    }
}