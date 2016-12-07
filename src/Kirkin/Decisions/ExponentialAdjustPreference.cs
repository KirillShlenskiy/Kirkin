using System;

namespace Kirkin.Decisions
{
    public class ExponentialAdjustPreference<TInput> : IPreference<TInput, double>
    {
        public IPreference<TInput, double> Source { get; }

        // Valid range: >0 to infinity.
        public double Power { get; }

        public string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        public ExponentialAdjustPreference(IPreference<TInput, double> source, double power)
        {
            if (power <= 0) throw new ArgumentOutOfRangeException(nameof(power));

            Source = source;
            Power = power;
        }

        public Decision<TInput, double> EstimateFitness(TInput input)
        {
            // Elimination:
            if (Power == 0.0) return Decision.Create(this, input, double.NaN);

            double intermediate = Source.EstimateFitness(input).Fitness;

            // Extremities:
            if (intermediate <= 0) return Decision.Create(Source, input, 0.0);
            if (intermediate >= 1.0) return Decision.Create(Source, input, 1.0);

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