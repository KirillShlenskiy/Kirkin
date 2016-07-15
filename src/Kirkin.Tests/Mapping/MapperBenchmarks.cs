using AutoMapper;

using Kirkin.Mapping;
using Kirkin.Reflection;

using Xunit;

namespace Kirkin.Tests.Mapping
{
    public class MapperBenchmarks
    {
        [Fact]
        public void AutomapperClone()
        {
            MapperConfiguration config = new MapperConfiguration(c => c.CreateMap<Dummy, Dummy>());
            IMapper mapper = config.CreateMapper();
            Dummy source = new Dummy { ID = 1, Value = "Blah" };

            for (int i = 0; i < 100000; i++) {
                Dummy target = mapper.Map<Dummy>(source);
            }
        }

        [Fact]
        public void KirkinMapperStaticClone()
        {
            Dummy source = new Dummy { ID = 1, Value = "Blah" };

            for (int i = 0; i < 100000; i++)
            {
                Dummy target = Kirkin.Mapping.Mapper.Clone(source);
            }
        }

        [Fact]
        public void KirkinMapperConfiguredAutomapClone()
        {
            Dummy source = new Dummy { ID = 1, Value = "Blah" };
            Mapper<Dummy, Dummy> mapper = new MapperBuilder<Dummy, Dummy>().BuildMapper();

            for (int i = 0; i < 100000; i++) {
                Dummy target = mapper.Map(source);
            }
        }

        [Fact]
        public void KirkinMapperConfiguredPropertyListClone()
        {
            Dummy source = new Dummy { ID = 1, Value = "Blah" };

            Mapper<Dummy, Dummy> mapper = MapperBuilder
                .FromPropertyList(PropertyList<Dummy>.Default)
                .ToPropertyList(PropertyList<Dummy>.Default)
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