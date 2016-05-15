using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// Base class for components which implement the IValidator interface.
    /// </summary>
    public abstract class ValidatorBase : IValidator
    {
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

            Validated?.Invoke(this, new ValidatedEventArgs(this, isValid));

            return isValid;
        }

        /// <summary>
        /// When overridden in a derived class, performs the validation and returns true if it is successful.
        /// This method's implementation should have no publically visible side-effects.
        /// </summary>
        protected abstract bool ValidateImpl();
    }
}