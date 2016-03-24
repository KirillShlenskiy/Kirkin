namespace Kirkin.Validation
{
    /// <summary>
    /// Contract for validators which act as a proxy for
    /// other validator(s) with optional event forwarding.
    /// </summary>
    internal interface IValidatorProxy : IValidator
    {
        /// <summary>
        /// Gets or sets the value which determines whether
        /// Validated events raised by the proxied validator(s) trigger
        /// the Validate call (and relevant events) on this instance.
        /// </summary>
        bool ForwardEvents { get; set; }
    }
}