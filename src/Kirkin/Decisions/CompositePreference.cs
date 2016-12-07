using System.Linq;

namespace Kirkin.Decisions
{
    public sealed class CompositePreference<TInput, TOutput>
        : IPreference<TInput, TOutput>
    {
        public string Name { get; }
        public IPreference<TInput, TOutput>[] Preferences { get; }

        public CompositePreference(string name, params IPreference<TInput, TOutput>[] criteria)
        {
            Name = name;
            Preferences = criteria;
        }

        public Decision<TInput, TOutput> EstimateFitness(TInput input)
        {
            Decision<TInput, TOutput>[] estimates = Preferences
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