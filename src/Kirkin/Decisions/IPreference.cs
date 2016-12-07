namespace Kirkin.Decisions
{
    public interface IPreference<TInput, TOutput>
    {
        Decision<TInput, TOutput> EstimateFitness(TInput input);
    }
}