using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// Validator whose state is manually controlled via its IsValid property.
    /// </summary>
    internal sealed class ManualValidator : ValidatorBase
    {
        internal bool? _isValid; // Internally visible for testing purposes.

        /// <summary>
        /// Gets or sets the state of this instance.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (!_isValid.HasValue) {
                    throw new InvalidOperationException("Validator state is undefined.");
                }

                return _isValid.GetValueOrDefault();
            }
            set
            {
                if (!_isValid.HasValue || value != _isValid.GetValueOrDefault())
                {
                    _isValid = value;

                    // Trigger events.
                    Validate();
                }
            }
        }

        /// <summary>
        /// Creates a validator whose state is undefined.
        /// Validate and IsValid getter will throw until
        /// IsValid is set.
        /// </summary>
        internal ManualValidator()
        {
        }

        /// <summary>
        /// Creates a validator with the given initial IsValid state.
        /// </summary>
        internal ManualValidator(bool isValid)
        {
            _isValid = isValid;
        }

        /// <summary>
        /// Returns the state of this instance.
        /// </summary>
        protected override bool ValidateImpl()
        {
            return IsValid;
        }
    }
}