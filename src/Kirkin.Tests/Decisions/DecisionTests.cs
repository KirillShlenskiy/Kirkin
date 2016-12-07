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

        sealed class EvenPreference : IPreference<double>
        {
            public Decision<double> EstimateFitness(double input)
            {
                return Decision.Create(this, input, input % 2 == 0 ? 1 : 0);
            }
        }

        [Fact]
        public void DecideSomething()
        {
            IPreference<double> makeItBig = new HigherIsBetterPreference(0, 10);
            IPreference<double> evenIsBetter = new EvenPreference();
            IPreference<double> pref = new CompositePreference<double>("Comp", makeItBig, evenIsBetter);

            for (int i = 0; i <= 10; i++)
            {
                IDecision decision = pref.EstimateFitness(i);

                Output.WriteLine($"{i} -> {decision}");
            }
        }
    }
}