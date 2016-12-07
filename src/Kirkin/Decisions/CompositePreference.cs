using System.Linq;

namespace Kirkin.Decisions
{
    public sealed class CompositePreference<TInput>
        : IPreference<TInput>
    {
        public string Name { get; }
        public IPreference<TInput>[] Preferences { get; }

        public CompositePreference(string name, params IPreference<TInput>[] criteria)
        {
            Name = name;
            Preferences = criteria;
        }

        public Decision<TInput> EstimateFitness(TInput input)
        {
            Decision<TInput>[] estimates = Preferences
                .Select(c => c.EstimateFitness(input))
                .ToArray();

            return Decision.Combine(this, input, estimates);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}