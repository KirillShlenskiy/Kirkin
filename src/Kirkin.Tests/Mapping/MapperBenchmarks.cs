using AutoMapper;

using Kirkin.Mapping;
using Kirkin.Reflection;

using NUnit.Framework;

using Mapper = Kirkin.Mapping.Mapper;

namespace Kirkin.Tests.Mapping
{
    public class MapperBenchmarks
    {
        [Test]
        public void AutomapperClone()
        {
            MapperConfiguration config = new MapperConfiguration(c => c.CreateMap<Dummy, Dummy>());
            IMapper mapper = config.CreateMapper();
            Dummy source = new Dummy { ID = 1, Value = "Blah" };

            for (int i = 0; i < 100000; i++) {
                Dummy target = mapper.Map<Dummy>(source);
            }
        }

        [Test]
        public void KirkinMapperStaticClone()
        {
            Dummy source = new Dummy { ID = 1, Value = "Blah" };

            for (int i = 0; i < 100000; i++) {
                Dummy target = Mapper.Clone(source);
            }
        }

        [Test]
        public void KirkinMapperConfiguredAutomapClone()
        {
            Dummy source = new Dummy { ID = 1, Value = "Blah" };
            Mapper<Dummy, Dummy> mapper = new MapperBuilder<Dummy, Dummy>().BuildMapper();

            for (int i = 0; i < 100000; i++) {
                Dummy target = mapper.Map(source);
            }
        }

        [Test]
        public void KirkinMapperConfiguredPropertyListClone()
        {
            Dummy source = new Dummy { ID = 1, Value = "Blah" };

            Mapper<Dummy, Dummy> mapper = Mapper.Builder
                .FromMembers(PropertyMember.PropertyListMembers(PropertyList<Dummy>.Default))
                .ToMembers(PropertyMember.PropertyListMembers(PropertyList<Dummy>.Default))
                .BuildMapper();

            for (int i = 0; i < 100000; i++) {
                Dummy target = mapper.Map(source);
            }
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}