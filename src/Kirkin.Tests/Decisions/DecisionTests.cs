using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kirkin.Decisions;

using Xunit;
using Xunit.Abstractions;

namespace Kirkin.Tests.Decisions
{
    public class DecisionTests
    {
        private readonly ITestOutputHelper Output;

        public DecisionTests(ITestOutputHelper output)
        {
            Output = output;
        }

        sealed class EvenPreference : IPreference<int, double>
        {
            public Decision<int, double> EstimateFitness(int input)
            {
                return Decision.Create(this, input, input % 2 == 0 ? 1 : 0);
            }
        }

        [Fact]
        public void DecideSomething()
        {
            IPreference<double, double> makeItBig = new HigherIsBetterPreference(0, 10);
            IPreference<int, double> evenIsBetter = new EvenPreference();

            IPreference<int, double> pref = new CompositePreference<int, double>(
                "Comp",
                new ProjectPreference<int, double, double>("Cast", i => i, makeItBig),
                evenIsBetter
            );

            for (int i = 0; i <= 10; i++)
            {
                IDecision decision = pref.EstimateFitness(i);

                Output.WriteLine($"{i} -> {decision}");
            }
        }
    }
}