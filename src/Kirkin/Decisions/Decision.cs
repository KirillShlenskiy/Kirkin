using System.Linq;

namespace Kirkin.Decisions
{
    /// <summary>
    /// <see cref="Decision{TInput, TOutput}"/> factory methods.
    /// </summary>
    public static class Decision
    {
        public static Decision<TInput, TOutput> Create<TInput, TOutput>(IPreference<TInput, TOutput> preference, TInput input, double fitness)
        {
            return new SimpleDecision<TInput, TOutput>(preference, input, fitness);
        }

        sealed class SimpleDecision<TInput, TOutput> : Decision<TInput, TOutput>
        {
            public override double Fitness { get; }

            public SimpleDecision(IPreference<TInput, TOutput> preference, TInput input, double fitness)
                : base(preference, input)
            {
                Fitness = fitness;
            }

            public override string ToString()
            {
                return $"{base.ToString()} ({Fitness:0.###})";
            }
        }

        public static Decision<TInput, TOutput> Wrap<TInput, TOutput>(IPreference<TInput, TOutput> preference, TInput input, IDecision inner)
        {
            return new WrapperDecision<TInput, TOutput>(preference, input, inner);
        }

        sealed class WrapperDecision<TInput, TOutput> : Decision<TInput, TOutput>
        {
            public IDecision Inner { get; }

            public override double Fitness
            {
                get
                {
                    return Inner.Fitness;
                }
            }

            public WrapperDecision(IPreference<TInput, TOutput> preference, TInput input, IDecision inner)
                : base(preference, input)
            {
                Inner = inner;
            }

            //public override string ToString()
            //{
            //    return $"{base.ToString()} wraps [{Inner}]";
            //}
        }

        public static Decision<TInput, TOutput> Combine<TInput, TOutput>(IPreference<TInput, TOutput> preference, TInput input, params Decision<TInput, TOutput>[] decisions)
        {
            return new CompositeDecision<TInput, TOutput>(preference, input, decisions);
        }

        sealed class CompositeDecision<TInput, TOutput> : Decision<TInput, TOutput>
        {
            public Decision<TInput, TOutput>[] Decisions { get; }

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

            public CompositeDecision(IPreference<TInput, TOutput> preference, TInput input, params Decision<TInput, TOutput>[] decisions)
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
    public abstract class Decision<TInput, TOutput>
        : IDecision
    {
        /// <summary>
        /// Preference used to calculate fitness.
        /// </summary>
        public IPreference<TInput, TOutput> Preference { get; }

        /// <summary>
        /// Input used to calculate fitness.
        /// </summary>
        public TInput Input { get; }

        /// <summary>
        /// This decision's relative measure of quality.
        /// </summary>
        public abstract double Fitness { get; }

        /// <summary>
        /// Creates a new <see cref="Decision{TInput, TOutput}"/> instance.
        /// </summary>
        protected Decision(IPreference<TInput, TOutput> preference, TInput input)
        {
            Preference = preference;
            Input = input;
        }

        /// <summary>
        /// Returns a string representation of this <see cref="Decision{TInput, TOutput}"/> instance.
        /// </summary>
        public override string ToString()
        {
            return $"{Preference}: {Fitness:0.000}";
        }
    }
}