namespace Kirkin.Decisions
{
    /// <summary>
    /// Preference computation result.
    /// </summary>
    public interface IDecision
    {
        /// <summary>
        /// This decision's relative measure of quality.
        /// </summary>
        double Fitness { get; }
    }
}