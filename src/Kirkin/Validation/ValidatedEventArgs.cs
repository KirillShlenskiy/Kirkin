using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// EventArgs which wrap an immutable validation result.
    /// </summary>
    public sealed class ValidatedEventArgs : EventArgs
    {
        /// <summary>
        /// Validator which raised the event.
        /// </summary>
        public IValidator Validator { get; }

        /// <summary>
        /// Gets the value returned by the validator's
        /// Validate call which caused the event to be raised.
        /// </summary>
        public bool IsValid { get; } // Immutable.

        internal ValidatedEventArgs(IValidator validator, bool isValid)
        {
            if (validator == null) throw new ArgumentNullException("validator");

            Validator = validator;
            IsValid = isValid;
        }
    }
}