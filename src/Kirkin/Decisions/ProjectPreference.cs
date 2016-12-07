using System;

namespace Kirkin.Decisions
{
    public sealed class ProjectPreference<TInput, TTransform, TOutput>
        : IPreference<TInput, TOutput>
    {
        public string Name { get; }
        public Func<TInput, TTransform> Projection { get; }
        public IPreference<TTransform, TOutput> Preference { get; }

        public ProjectPreference(string name,
                                 Func<TInput, TTransform> projection,
                                 IPreference<TTransform, TOutput> preference)
        {
            Name = name;
            Projection = projection;
            Preference = preference;
        }

        public Decision<TInput, TOutput> EstimateFitness(TInput input)
        {
            TTransform intermediateInput = Projection(input);
            IDecision intermediateDecision = Preference.EstimateFitness(intermediateInput);

            return Decision.Wrap(this, input, intermediateDecision);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}