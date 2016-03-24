using System.Collections.Generic;
using System.Collections.Immutable;

using Xunit;

namespace Kirkin.Tests.Collections.Generic
{
    public class CollectionIterationTests
    {
        static readonly Dummy[] Array = CreateArray();
        static readonly ImmutableArray<Dummy> ImmutableArr = CreateImmutableArray();
        static readonly List<Dummy> List = CreateList();

        [Fact]
        public void ArrayIterationPerf()
        {
            for (int i = 0; i < 1000; i++)
            {
                foreach (var dummy in Array)
                {
                    var id = dummy.ID;
                }
            }
        }

        [Fact]
        public void ImmutableArrayIterationPerf()
        {
            for (int i = 0; i < 1000; i++)
            {
                foreach (var dummy in ImmutableArr)
                {
                    var id = dummy.ID;
                }
            }
        }

        [Fact]
        public void ListIterationPerf()
        {
            for (int i = 0; i < 1000; i++)
            {
                foreach (var dummy in List)
                {
                    var id = dummy.ID;
                }
            }
        }

        static List<Dummy> CreateList()
        {
            var list = new List<Dummy>(5000);

            for (int i = 0; i < list.Count; i++) {
                list[i] = new Dummy { ID = i, Value = i.ToString() };
            }

            return list;
        }

        static Dummy[] CreateArray()
        {
            var array = new Dummy[5000];

            for (int i = 0; i < array.Length; i++) {
                array[i] = new Dummy { ID = i, Value = i.ToString() };
            }

            return array;
        }

        static ImmutableArray<Dummy> CreateImmutableArray()
        {
            var array = ImmutableArray.CreateBuilder<Dummy>(5000);

            for (int i = 0; i < array.Capacity; i++) {
                array.Add(new Dummy { ID = i, Value = i.ToString() });
            }

            return array.ToImmutable();
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}
