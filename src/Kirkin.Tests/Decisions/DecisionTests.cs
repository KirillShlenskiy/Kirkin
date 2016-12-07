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

        sealed class EvenPreference : IPreference<int>
        {
            public Decision<int> EstimateFitness(int input)
            {
                return Decision.Create(this, input, input % 2 == 0 ? 1 : 0);
            }
        }

        [Fact]
        public void DecideSomething()
        {
            IPreference<int> makeItBig = new HigherIsBetterPreference(0, 10).WithInputConversion((int i) => i);
            IPreference<int> evenIsBetter = new EvenPreference();
            IPreference<int> pref = new CompositePreference<int>("Comp", makeItBig, evenIsBetter);

            for (int i = 0; i <= 10; i++)
            {
                IDecision decision = pref.EstimateFitness(i);

                Output.WriteLine($"{i} -> {decision}");
            }
        }
    }
}