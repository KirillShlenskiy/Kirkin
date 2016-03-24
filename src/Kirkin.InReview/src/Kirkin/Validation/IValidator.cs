using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// Common interface implemented by validator components.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Raised when the component has finished performing all validation.
        /// The IsValid value of the event args is final and cannot change.
        /// </summary>
        event EventHandler<ValidatedEventArgs> Validated;

        /// <summary>
        /// Performs validation and returns true if it is successful.
        /// </summary>
        bool Validate();
    }
}