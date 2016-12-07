using System;

namespace Kirkin.Decisions
{
    public sealed class MultiplyPreference : IPreference<double, double>
    {
        public IPreference<double, double> Source { get; }
        public double Multiplier { get; }

        public MultiplyPreference(IPreference<double, double> source, double multiplier)
        {
            if (multiplier < 0 || multiplier > 1) throw new ArgumentOutOfRangeException(nameof(multiplier));

            Source = source;
            Multiplier = multiplier;
        }

        public Decision<double, double> EstimateFitness(double input)
        {
            // Elimination:
            if (Multiplier == 0.0) return Decision.Create(this, input, double.NaN);

            // Calc:
            double intermediate = Source.EstimateFitness(input).Fitness;
            double fitness = intermediate * Multiplier;

            return Decision.Create(this, input, fitness);
        }

        public override string ToString()
        {
            return $"Linear: {Multiplier}x{Source}";
        }
    }
}