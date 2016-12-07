namespace Kirkin.Decisions
{
    public sealed class LowerIsBetterPreference : IPreference<double>
    {
        public double MinValue { get; }
        public double MaxValue { get; }

        public LowerIsBetterPreference(double minValue, double maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public Decision<double> EstimateFitness(double input)
        {
            // Elimination:
            if (MinValue == MaxValue) return Decision.Create(this, input, double.NaN);

            // Extremities:
            if (input <= MinValue) return Decision.Create(this, input, 1.0);
            if (input >= MaxValue) return Decision.Create(this, input, 0.0);

            // Calc:
            double fitness = 1.0 - (input - MinValue) / (MaxValue - MinValue);

            return Decision.Create(this, input, fitness);
        }

        public override string ToString()
        {
            return $"Lower is better: {MinValue} - {MaxValue}";
        }
    }
}