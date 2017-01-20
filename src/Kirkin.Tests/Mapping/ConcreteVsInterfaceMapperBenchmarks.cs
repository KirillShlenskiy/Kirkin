using BenchmarkDotNet.Attributes;

using Kirkin.Tests.Benchmarking;
using Kirkin.Mapping;

using NUnit.Framework;

namespace Kirkin.Tests.Mapping
{
    public sealed class ConcreteVsInterfaceMapperBenchmarks
    {
        //[Test]
        public void RunBenchmarks()
        {
            string report = Benchmarks.Run<MapperBenchmarks>();

            Output.WriteLine(report);
        }

        public class MapperBenchmarks
        {
            private readonly Mapper<Dummy, Dummy> Map = new MapperBuilder<Dummy, Dummy>().BuildMapper();
            private readonly Dummy Source = new Dummy { ID = 123, Value = "Blah" };
            private readonly Dummy Target = new Dummy();

            [Setup]
            public void SetUp()
            {
                Map.Map(Source, Target);
            }

            [Benchmark]
            public void ConcreteMapper()
            {
                Mapper<Dummy, Dummy> mapper = Map;

                mapper.Map(Source, Target);
            }

            [Benchmark]
            public void InterfaceMapper()
            {
                Mapper<Dummy, Dummy> mapper = Map;

                mapper.Map(Source, Target);
            }
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}