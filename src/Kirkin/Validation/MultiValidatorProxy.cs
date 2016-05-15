using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// IValidator implementation which wraps one or more IValidator
    /// instances, proxies their Validate calls and aggregates events.
    /// </summary>
    internal sealed class MultiValidatorProxy : IValidatorProxy
    {
        /// <summary>
        /// Prevents re-entrant calls to Validate(ValidationResult).
        /// </summary>
        private bool IsValidating;

        /// <summary>
        /// Gets or sets the value which determines whether
        /// Validated events raised by child validators trigger
        /// the Validate call (and relevant events) on this instance.
        /// The default is true.
        /// </summary>
        public bool ForwardEvents { get; set; }

        /// <summary>
        /// Collection of validators wrapped by this instance.
        /// </summary>
        public IValidator[] Validators { get; }

        /// <summary>
        /// Raised when the component has finished performing all validation.
        /// The IsValid value of the event args is final and cannot change.
        /// </summary>
        public event EventHandler<ValidatedEventArgs> Validated;

        /// <summary>
        /// Creates a new validator which wraps one or more IValidator
        /// instances, proxies their Validate calls and aggregates events.
        /// </summary>
        internal MultiValidatorProxy(IValidator[] validators)
        {
            Validators = validators;

            // Defaults.
            ForwardEvents = true;

            // Wire events.
            foreach (var validator in validators)
            {
                validator.Validated += (s, e) =>
                {
                    if (ForwardEvents)
                    {
                        // If Validate is called on MultiValidator,
                        // this check will fail for all child validators.
                        // If Validate is called on one of the child validators,
                        // it will succeed for that one validator and fail for the
                        // rest, thereby ensuring that Validate is only called once.
                        if (!IsValidating) {
                            Validate(new ValidationResult(e.Validator, e.IsValid));
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Performs validation and returns true if it is successful.
        /// </summary>
        public bool Validate()
        {
            return Validate(default(ValidationResult));
        }

        /// <summary>
        /// Validation implementation.
        /// </summary>
        private bool Validate(ValidationResult alreadyValidated)
        {
            bool isValid = true;

            // Prevent reentrancy.
            IsValidating = true;

            try
            {
                foreach (var validator in Validators)
                {
                    if (validator == alreadyValidated.Validator)
                    {
                        // We already have the result. Do not call Validate again.
                        isValid &= alreadyValidated.IsValid;
                    }
                    else
                    {
                        // No short-cicuiting. Every child validator (except
                        // the one already validated) must have Validate called.
                        isValid &= validator.Validate();
                    }
                }

                Validated?.Invoke(this, new ValidatedEventArgs(this, isValid));
            }
            finally
            {
                IsValidating = false;
            }

            return isValid;
        }

        struct ValidationResult
        {
            internal readonly IValidator Validator;
            internal readonly bool IsValid;

            internal ValidationResult(IValidator validator, bool isValid)
            {
                Validator = validator;
                IsValid = isValid;
            }
        }
    }
}