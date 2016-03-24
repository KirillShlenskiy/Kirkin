namespace Kirkin.Validation
{
    /// <summary>
    /// Interface implemented by complex validation side effects.
    /// </summary>
    internal interface ISideEffect
    {
        /// <summary>
        /// Applies the side effect based on the validation result.
        /// </summary>
        void Apply(bool isValid); // Must have a signature compatible with SideEffectDelegate.
    }
}