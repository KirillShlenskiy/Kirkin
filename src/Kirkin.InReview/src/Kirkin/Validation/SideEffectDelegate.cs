namespace Kirkin.Validation
{
    /// <summary>
    /// Encapsulates an action which accepts the
    /// validation result as its only parameter.
    /// </summary>
    public delegate void SideEffectDelegate(bool isValid);
}