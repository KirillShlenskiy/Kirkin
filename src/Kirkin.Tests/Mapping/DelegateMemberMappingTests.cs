using System.Collections.Generic;
using System.Linq;

using Kirkin.Mapping;

using NUnit.Framework;

namespace Kirkin.Tests.Mapping
{
    public class DelegateMemberMappingTests
    {
        [Test]
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

            Assert.AreEqual(123, d.ID);
            Assert.AreEqual("Test", d.Value);
        }

        [Test]
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

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(123, values["ID"]);
            Assert.AreEqual("Test", values["Value"]);

            d.ID = 321;
            d.Value = "tseT";

            mapper.Map(d, values);

            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(321, values["ID"]);
            Assert.AreEqual("tseT", values["Value"]);
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }

        [Test]
        public void NonGenericGetterAndSetter()
        {
            Member<Dummy>[] members = {
                DelegateMember.ReadWrite<Dummy>("ID", typeof(int), d => d.ID, (d, id) => d.ID = (int)id),
                DelegateMember.ReadWrite<Dummy>("Value", typeof(string), d => d.Value, (d, value) => d.Value = (string)value),
            };

            Mapper<Dummy, Dummy> mapper = new MapperBuilder<Dummy, Dummy>(members, members).BuildMapper();

            Dummy dummy1 = new Dummy {
                ID = 123,
                Value = "Zzz"
            };

            Dummy dummy2 = mapper.Map(dummy1);

            Assert.AreNotSame(dummy1, dummy2);
            Assert.AreEqual(123, dummy2.ID);
            Assert.AreEqual("Zzz", dummy2.Value);
        }

        [Test]
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
                            Member<Dummy> changeTrackingMemberDecorator = DelegateMember.ReadWrite<Dummy>(
                                m.Name,
                                m.MemberType,
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

            Assert.AreEqual(0, changeCount);

            dummy1.ID = 123;
            dummy1.Value = "Zzz";

            mapper.Map(dummy1, dummy2);

            Assert.AreEqual(123, dummy2.ID);
            Assert.AreEqual("Zzz", dummy2.Value);
            Assert.AreEqual(2, changeCount);

            dummy2 = new Dummy();

            mapper.Map(dummy1, dummy2);

            Assert.AreEqual(4, changeCount); // No reset.
        }

        [Test]
        public void SimulateTypeMappingMapAndCountChangesWithCustomWrapperType()
        {
            int changeCount = 0;

            Mapper<Dummy, ChangeCounter<Dummy>> mapper = Mapper.Builder
                .FromPublicInstanceProperties<Dummy>()
                .ToMembers(
                    PropertyMember
                        .PublicInstanceProperties<Dummy>()
                        .Select(m =>
                        {
                            Member<ChangeCounter<Dummy>> changeTrackingMemberDecorator = DelegateMember.ReadWrite<ChangeCounter<Dummy>>(
                                m.Name,
                                m.MemberType,
                                d => m.Property.GetValue(d.Target),
                                (d, v) =>
                                {
                                    object currentValue = m.Property.GetValue(d.Target);

                                    if (!Equals(v, currentValue))
                                    {
                                        m.Property.SetValue(d.Target, v);

                                        d.ChangeCount++;
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

            changeCount = mapper.Map(dummy1, new ChangeCounter<Dummy>(dummy2)).ChangeCount;

            Assert.AreEqual(0, changeCount);

            dummy1.ID = 123;
            dummy1.Value = "Zzz";

            changeCount = mapper.Map(dummy1, new ChangeCounter<Dummy>(dummy2)).ChangeCount;

            Assert.AreEqual(123, dummy2.ID);
            Assert.AreEqual("Zzz", dummy2.Value);
            Assert.AreEqual(2, changeCount);

            dummy2 = new Dummy();

            changeCount = mapper.Map(dummy1, new ChangeCounter<Dummy>(dummy2)).ChangeCount;

            Assert.AreEqual(2, changeCount); // No reset.
        }

        class ChangeCounter<T>
        {
            public T Target { get; }
            public int ChangeCount { get; set;}

            public ChangeCounter(T target)
            {
                Target = target;
                ChangeCount = 0;
            }
        }
    }
}