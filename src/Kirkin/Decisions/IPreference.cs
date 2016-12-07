namespace Kirkin.Decisions
{
    public interface IPreference<TInput>
    {
        Decision<TInput> EstimateFitness(TInput input);
    }
}