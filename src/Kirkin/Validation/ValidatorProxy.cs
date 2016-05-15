using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// IValidator implementation which wraps another IValidator
    /// instance, proxies its Validate call and forwards events.
    /// </summary>
    internal sealed class ValidatorProxy : IValidatorProxy
    {
        /// <summary>
        /// Prevents re-entrant calls to Validate(ValidationResult).
        /// </summary>
        private bool IsValidating;

        /// <summary>
        /// Gets or sets the value which determines whether
        /// Validated events raised by child validator trigger
        /// the Validate call (and relevant events) on this instance.
        /// The default is true.
        /// </summary>
        public bool ForwardEvents { get; set; }

        /// <summary>
        /// Validator wrapped by this instance.
        /// </summary>
        public IValidator Validator { get; }

        /// <summary>
        /// Raised when the component has finished performing all validation.
        /// The IsValid value of the event args is final and cannot change.
        /// </summary>
        public event EventHandler<ValidatedEventArgs> Validated;

        /// <summary>
        /// Creates a new validator which wraps one or more IValidator
        /// instances, proxies their Validate calls and aggregates events.
        /// </summary>
        internal ValidatorProxy(IValidator validator)
        {
            Validator = validator;

            // Defaults.
            ForwardEvents = true;

            // Wire events.
            validator.Validated += (s, e) =>
            {
                if (ForwardEvents && !IsValidating) {
                    Validate(e.IsValid);
                }
            };
        }

        /// <summary>
        /// Performs validation and returns true if it is successful.
        /// </summary>
        public bool Validate()
        {
            return Validate(null);
        }

        /// <summary>
        /// Validation implementation.
        /// </summary>
        private bool Validate(bool? readyIsValid)
        {
            bool isValid;

            // Prevent reentrancy.
            IsValidating = true;

            try
            {
                isValid = readyIsValid ?? Validator.Validate();

                if (Validated != null) {
                    Validated(this, new ValidatedEventArgs(this, isValid));
                }
            }
            finally
            {
                IsValidating = false;
            }

            return isValid;
        }
    }
}