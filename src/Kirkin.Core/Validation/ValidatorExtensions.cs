using System;

namespace Kirkin.Validation
{
    /// <summary>
    /// Contains common validator extensions.
    /// </summary>
    public static class ValidatorExtensions
    {
        /// <summary>
        /// Returns a new validator instance which proxies this instance's Validate
        /// method call and combines it with additional validation logic to produce
        /// the final result. The original instance's Validate behaviour will be
        /// preserved. Calling Validate on the original instance will automatically
        /// cause the validation of the new instance, and vice versa.
        /// </summary>
        public static IValidator WithExtraValidation(this IValidator validator, Func<bool> extraValidation)
        {
            if (extraValidation == null) throw new ArgumentNullException("extraValidation");

            return new MultiValidatorProxy(new[] { validator, new CustomValidator(extraValidation) });
        }

        /// <summary>
        /// Returns a new validator instance which proxies this instance's Validate
        /// method call and executes the given side effect when the result is available.
        /// The original instance's Validate behaviour will be preserved.
        /// Calling Validate on the original instance will automatically
        /// cause the validation of the new instance, and vice versa.
        /// </summary>
        internal static IValidator WithSideEffect(this IValidator validator, ISideEffect sideEffect)
        {
            if (sideEffect == null) throw new ArgumentNullException("sideEffect");

            var proxy = Validator.Proxy(validator);

            proxy.Validated += (s, e) => sideEffect.Apply(e.IsValid);

            return proxy;
        }

        /// <summary>
        /// Returns a new validator instance which proxies this instance's Validate
        /// method call and executes the given side effect when the result is available.
        /// The original instance's Validate behaviour will be preserved.
        /// Calling Validate on the original instance will automatically
        /// cause the validation of the new instance, and vice versa.
        /// </summary>
        public static IValidator WithSideEffect(this IValidator validator, SideEffectDelegate sideEffect)
        {
            if (sideEffect == null) throw new ArgumentNullException("sideEffect");

            var proxy = Validator.Proxy(validator);

            proxy.Validated += (s, e) => sideEffect(e.IsValid);

            return proxy;
        }
    }
}