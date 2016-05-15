using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// EventArgs which wrap a mutable validation result.
    /// </summary>
    internal sealed class ValidatingEventArgs : EventArgs
    {
        /// <summary>
        /// Validator which raised the event.
        /// </summary>
        public IValidator Validator { get; }

        /// <summary>
        /// Gets or sets the value to be returned by the validator's
        /// Validate call which caused the event to be raised.
        /// </summary>
        public bool IsValid { get; set; } // Mutable.

        internal ValidatingEventArgs(IValidator validator, bool isValid)
        {
            if (validator == null) throw new ArgumentNullException("validator");

            Validator = validator;
            IsValid = isValid;
        }
    }
}