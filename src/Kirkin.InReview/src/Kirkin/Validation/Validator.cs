using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// Contains common validator factory methods.
    /// </summary>
    public static class Validator
    {
        /// <summary>
        /// Combines multiple IValidator objects into a single instance.
        /// Calling Validate on the new instance will result in Validate
        /// calls on each child validator. Any events raised by the child
        /// validators will be forwarded by the newly created instance.
        /// </summary>
        public static IValidator Combine(params IValidator[] validators)
        {
            if (validators == null) throw new ArgumentNullException("validators");
            if (validators.Length == 0) throw new ArgumentException("validators");

            IValidator[] defensiveCopy = new IValidator[validators.Length];

            Array.Copy(validators, 0, defensiveCopy, 0, validators.Length);

            return new MultiValidatorProxy(defensiveCopy);
        }

        /// <summary>
        /// Returns a validator with the given initial valid state.
        /// </summary>
        internal static IValidator Create(bool isValid)
        {
            return new ManualValidator(isValid);
        }

        /// <summary>
        /// Returns an IValidator implementation which is driven by the given custom validation delegate.
        /// </summary>
        public static IValidator Create(Func<bool> validation)
        {
            if (validation == null) throw new ArgumentNullException("validation");

            return new CustomValidator(validation);
        }

        /// <summary>
        /// Returns a proxy of the given IValidator instance which
        /// optionally forwards the original instance's events (on by default).
        /// </summary>
        internal static IValidatorProxy Proxy(IValidator validator)
        {
            if (validator == null) throw new ArgumentNullException("validator");

            return new ValidatorProxy(validator);
        }
    }
}