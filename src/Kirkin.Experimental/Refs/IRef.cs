namespace Kirkin.Refs
{
    /// <summary>
    /// Reference to a value.
    /// </summary>
    internal interface IRef
    {
        /// <summary>
        /// Gets or sets the value that this reference is pointing to.
        /// </summary>
        object Value { get; set; }
    }
}