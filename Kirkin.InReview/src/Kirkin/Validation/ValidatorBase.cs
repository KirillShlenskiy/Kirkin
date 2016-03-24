using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// Base class for components which implement the IValidator interface.
    /// </summary>
    public abstract class ValidatorBase : IValidator
    {
        /// <summary>
        /// Raised as soon as the component has finished performing initial
        /// validation. Allows subscribers to change the validation result.
        /// </summary>
        internal event EventHandler<ValidatingEventArgs> Validating;

        /// <summary>
        /// Raised when the component has finished performing all validation.
        /// The IsValid value of the event args is final and cannot change.
        /// </summary>
        public event EventHandler<ValidatedEventArgs> Validated;

        /// <summary>
        /// Performs validation and returns true if it is successful.
        /// </summary>
        public bool Validate()
        {
            // Resolve initial value.
            bool isValid = ValidateImpl();

            if (Validating != null)
            {
                // Allow subscribers to perform additional validation.
                var validatingArgs = new ValidatingEventArgs(this, isValid);

                Validating(this, validatingArgs);

                // Could have been changed by event subscribers.
                isValid = validatingArgs.IsValid;
            }

            if (Validated != null) {
                Validated(this, new ValidatedEventArgs(this, isValid));
            }

            return isValid;
        }

        /// <summary>
        /// When overridden in a derived class, performs the validation and returns true if it is successful.
        /// This method's implementation should have no publically visible side-effects.
        /// </summary>
        protected abstract bool ValidateImpl();
    }
}