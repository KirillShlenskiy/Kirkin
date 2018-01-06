using System;

using Kirkin.DataAnalysis;

using NUnit.Framework;

namespace Kirkin.Tests.DataAnalysis
{
    public class DataAnalysisTests
    {
        class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }

        [Test]
        public void BasicDataAnalysis()
        {
            Dummy[] dummies = {
                new Dummy { ID = 1, Value = "Blah" },
                new Dummy { ID = 2, Value = "Blah" },
                new Dummy { ID = 3, Value = "Blah" },
                new Dummy { ID = 4, Value = "Blah" },
                new Dummy { ID = 5, Value = "Blah" },
                new Dummy { ID = 6, Value = "Blah" }
            };

            DynamicInsightFactory factory = new DynamicInsightFactory();
            DataInsight idInsight = factory.GenerateInsights(PropertyValueSet.FromCollection(dummies, d => d.ID));
            DataInsight valueInsight = factory.GenerateInsights(PropertyValueSet.FromCollection(dummies, d => d.Value));

            Console.WriteLine(idInsight.ToString());
            Console.WriteLine(valueInsight.ToString());
        }
    }
}