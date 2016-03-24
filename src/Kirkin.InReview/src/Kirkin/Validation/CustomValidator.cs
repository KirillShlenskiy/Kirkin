using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// IValidator implementation which uses a delegate to perform validation.
    /// </summary>
    internal sealed class CustomValidator : ValidatorBase
    {
        private readonly Func<bool> Validation;

        /// <summary>
        /// Creates a new instance of the validator using the given
        /// delegate to perform validation. The delegate should
        /// have no publically visible side-effects.
        /// </summary>
        internal CustomValidator(Func<bool> validation)
        {
            Validation = validation;
        }

        /// <summary>
        /// Performs validation and returns true if it is successful.
        /// </summary>
        protected override bool ValidateImpl()
        {
            return Validation();
        }
    }
}