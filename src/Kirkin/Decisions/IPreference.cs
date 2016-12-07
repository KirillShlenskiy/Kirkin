namespace Kirkin.Decisions
{
    public interface IPreference<TInput> // No TOutput parameter because decision quality is always of type Double.
    {
        Decision<TInput> EstimateFitness(TInput input);
    }
}