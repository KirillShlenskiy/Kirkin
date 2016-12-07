using System;

namespace Kirkin.Decisions
{
    public class ExponentialAdjustPreference : IPreference<double>
    {
        public IPreference<double> Source { get; }

        // Valid range: >0 to infinity.
        public double Power { get; }

        public string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        public ExponentialAdjustPreference(IPreference<double> source, double power)
        {
            if (power <= 0) throw new ArgumentOutOfRangeException(nameof(power));

            Source = source;
            Power = power;
        }

        public Decision<double> EstimateFitness(double input)
        {
            // Elimination:
            if (Power == 0.0) return Decision.Create(this, input, double.NaN);

            double intermediate = Source.EstimateFitness(input).Fitness;

            // Extremities:
            if (intermediate <= 0) return Decision.Create(this, intermediate, 0.0);
            if (intermediate >= 1.0) return Decision.Create(this, intermediate, 1.0);

            // Curve: y = 1 - sqrt[n](1 - x ^ n).
            double fitness = 1.0 - RootToThePower(1.0 - Math.Pow(intermediate, Power), Power);

            return Decision.Create(this, input, fitness);
        }

        static double RootToThePower(double value, double power)
        {
            return Math.Pow(value, 1.0 / power);
        }

        public override string ToString()
        {
            return $"Exponential: {Power}x[{Source}]";
        }
    }
}