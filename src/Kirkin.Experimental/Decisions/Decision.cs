using System.Linq;

namespace Kirkin.Decisions
{
    /// <summary>
    /// <see cref="Decision{TInput}"/> factory methods.
    /// </summary>
    public static class Decision
    {
        /// <summary>
        /// Creates a simple decision with the given fitness value
        /// based on the given preference and input combination.
        /// </summary>
        public static Decision<TInput> Create<TInput>(IPreference<TInput> preference, TInput input, double fitness)
        {
            return new SimpleDecision<TInput>(preference, input, fitness);
        }

        sealed class SimpleDecision<TInput> : Decision<TInput>
        {
            public override double Fitness { get; }

            public SimpleDecision(IPreference<TInput> preference, TInput input, double fitness)
                : base(preference, input)
            {
                Fitness = fitness;
            }

            public override string ToString()
            {
                return $"{base.ToString()} ({Fitness:0.###})";
            }
        }

        /// <summary>
        /// Decorates a decision with new preference and input values.
        /// </summary>
        public static Decision<TInput> Wrap<TInput>(IPreference<TInput> preference, TInput input, IDecision inner)
        {
            return new WrapperDecision<TInput>(preference, input, inner);
        }

        sealed class WrapperDecision<TInput> : Decision<TInput>
        {
            public IDecision Inner { get; }

            public override double Fitness
            {
                get
                {
                    return Inner.Fitness;
                }
            }

            public WrapperDecision(IPreference<TInput> preference, TInput input, IDecision inner)
                : base(preference, input)
            {
                Inner = inner;
            }
        }

        /// <summary>
        /// Creates a composite decision whose fitness is the average
        /// of the fitness values of its constituent decisions.
        /// </summary>
        public static Decision<TInput> Combine<TInput>(IPreference<TInput> preference, TInput input, params Decision<TInput>[] decisions)
        {
            return new CompositeDecision<TInput>(preference, input, decisions);
        }

        sealed class CompositeDecision<TInput> : Decision<TInput>
        {
            public Decision<TInput>[] Decisions { get; }

            public override double Fitness
            {
                get
                {
                    double sum = 0.0;

                    foreach (IDecision estimate in Decisions)
                    {
                        if (!double.IsNaN(estimate.Fitness)) {
                            sum += estimate.Fitness;
                        }
                    }

                    return sum / Decisions.Length;
                }
            }

            public CompositeDecision(IPreference<TInput> preference, TInput input, params Decision<TInput>[] decisions)
                : base(preference, input)
            {
                Decisions = decisions;
            }

            public override string ToString()
            {
                return $"{base.ToString()} ({string.Join(", ", Decisions.Select(v => v.ToString()))})";
            }
        }
    }

    /// <summary>
    /// Encapsulates a decision about the fitness of the given
    /// input from with regards to a particular preference.
    /// </summary>
    public abstract class Decision<TInput>
        : IDecision
    {
        /// <summary>
        /// Preference used to calculate fitness.
        /// </summary>
        public IPreference<TInput> Preference { get; }

        /// <summary>
        /// Input used to calculate fitness.
        /// </summary>
        public TInput Input { get; }

        /// <summary>
        /// This decision's relative measure of quality.
        /// </summary>
        public abstract double Fitness { get; }

        /// <summary>
        /// Creates a new <see cref="Decision{TInput}"/> instance.
        /// </summary>
        protected Decision(IPreference<TInput> preference, TInput input)
        {
            Preference = preference;
            Input = input;
        }

        /// <summary>
        /// Returns a string representation of this <see cref="Decision{TInput}"/> instance.
        /// </summary>
        public override string ToString()
        {
            return $"{Preference}: {Fitness:0.000}";
        }
    }
}