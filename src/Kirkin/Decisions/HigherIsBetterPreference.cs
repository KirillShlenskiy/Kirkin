namespace Kirkin.Decisions
{
    public sealed class HigherIsBetterPreference : IPreference<double, double>
    {
        public double MinValue { get; }
        public double MaxValue { get; }

        public HigherIsBetterPreference(double minValue, double maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public Decision<double, double> EstimateFitness(double input)
        {
            // Elimination:
            if (MinValue == MaxValue) return Decision.Create(this, input, double.NaN);

            // Extremities:
            if (input >= MaxValue) return Decision.Create(this, input, 1.0);
            if (input <= MinValue) return Decision.Create(this, input, 0.0);

            // Calc:
            double fitness = (input - MinValue) / (MaxValue - MinValue);

            return Decision.Create(this, input, fitness);
        }

        public override string ToString()
        {
            return $"Higher is better: {MinValue} - {MaxValue}";
        }
    }
}