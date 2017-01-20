using System;

using Kirkin.Validation;

using NUnit.Framework;

namespace Kirkin.Tests.Validation
{
    [TestFixture]
    public class ValidatorTests
    {
        [Test]
        public void ArgumentValidation()
        {
            // Constructors.
            Assert.Throws<ArgumentNullException>(() => Validator.Create(null));
            Assert.Throws<ArgumentNullException>(() => Validator.Combine(null));

            // EventArgs.
            Assert.Throws<ArgumentNullException>(() => new ValidatedEventArgs(null, false));

            // Extensions.
            Assert.Throws<ArgumentNullException>(() => Validator.Create(() => true).WithExtraValidation(null));
            Assert.Throws<ArgumentNullException>(() => Validator.Create(() => true).WithSideEffect((ISideEffect)null));
            Assert.Throws<ArgumentNullException>(() => Validator.Create(() => true).WithSideEffect((SideEffectDelegate)null));
        }

        [Test]
        public void ManualValidator()
        {
            // Unspecified initial state.
            var validator = new ManualValidator();
            int validateCount = 0;

            validator.Validated += (s, e) => validateCount++;

            bool ignored;

            Assert.Throws<InvalidOperationException>(() => ignored = validator.IsValid);
            Assert.Throws<InvalidOperationException>(() => validator.Validate());

            validator.IsValid = true; // Will raise.

            Assert.True(validator.IsValid);
            Assert.AreEqual(1, validateCount);

            Assert.True(validator.Validate());
            Assert.AreEqual(2, validateCount);

            validator.IsValid = false; // Will raise.

            Assert.False(validator.IsValid);
            Assert.AreEqual(3, validateCount);

            validator.IsValid = false; // Will not raise.

            Assert.False(validator.IsValid);
            Assert.AreEqual(3, validateCount);

            Assert.False(validator.Validate());
            Assert.AreEqual(4, validateCount);

            // Specified initial state.
            validator = new ManualValidator(true);

            validator.Validated += (s, e) => validateCount++;

            Assert.True(validator.IsValid);

            validator.IsValid = true; // Will not raise.

            Assert.AreEqual(4, validateCount);
        }

        [Test]
        public void Multi()
        {
            var validator1 = Validator.Create(true);
            var validator2 = new CustomValidator(() => true);
            var multi = new MultiValidatorProxy(new[] { validator1, validator2 });

            // Count diagnostics.
            // Any time Validate is called on multi,
            // all validated counts will go up by one.
            // Any time Validate is called on a child validator,
            // all validated counts (inc multi) will go up by one.
            // In the end all counts are expected to be the same.
            int validator1ValidatedCount = 0;
            int validator2ValidatedCount = 0;
            int multiValidatedCount = 0;
            bool multiResult = false;

            validator1.Validated += (s, e) => validator1ValidatedCount++;
            validator2.Validated += (s, e) => validator2ValidatedCount++;

            multi.Validated += (s, e) =>
            {
                multiResult = e.IsValid;
                multiValidatedCount++;
            };

            // First Validate (via multi).
            Assert.True(multi.Validate());
            Assert.True(multiResult);
            Assert.AreEqual(1, multiValidatedCount);
            Assert.AreEqual(multiValidatedCount, validator1ValidatedCount);
            Assert.AreEqual(multiValidatedCount, validator2ValidatedCount);

            // Second Validate (via validator1).
            Assert.True(validator1.Validate());
            Assert.True(multiResult);
            Assert.AreEqual(2, multiValidatedCount);
            Assert.AreEqual(multiValidatedCount, validator1ValidatedCount);
            Assert.AreEqual(multiValidatedCount, validator2ValidatedCount);

            //// Third Validate (via validator1, validator2 result overridden).
            //validator2.Validating += (s, e) => e.IsValid = false; // Override.

            //Assert.True(validator1.Validate());
            //Assert.False(multiResult); // Due to validator2 isValid = false.
            //Assert.AreEqual(3, multiValidatedCount);
            //Assert.AreEqual(multiValidatedCount, validator1ValidatedCount);
            //Assert.AreEqual(multiValidatedCount, validator2ValidatedCount);

            //// Multi overriding.
            //multi.Validating += (s, e) => e.IsValid = true;

            //Assert.True(multi.Validate());
            //Assert.True(multiResult);
            //Assert.AreEqual(4, multiValidatedCount);
            //Assert.AreEqual(multiValidatedCount, validator1ValidatedCount);
            //Assert.AreEqual(multiValidatedCount, validator2ValidatedCount);
        }

        [Test]
        public void ProxyEventForwardingOnByDefault()
        {
            IValidator validator = Validator.Create(() => true);
            IValidatorProxy proxy = Validator.Proxy(validator);

            Assert.True(proxy.ForwardEvents);
        }

        [Test]
        public void WithExtraValidation()
        {
            IValidator validator1 = Validator.Create(() => true);
            Assert.True(validator1.Validate());

            IValidator validator2 = validator1.WithExtraValidation(() => true);
            Assert.AreNotSame(validator1, validator2); // Ensure no mutation.
            Assert.True(validator1.Validate());
            Assert.True(validator2.Validate());

            IValidator validator3 = validator2.WithExtraValidation(() => false);
            Assert.True(validator1.Validate());
            Assert.True(validator2.Validate());
            Assert.False(validator3.Validate());
        }

        [Test]
        public void WithExtraValidationAndSideEffectCascade()
        {
            int validateCount = 0;
            var validator1 = new ManualValidator(true);
            var validator2 = (IValidatorProxy)validator1.WithExtraValidation(() => true).WithSideEffect(_ => validateCount++);

            validator1.Validate();
            Assert.AreEqual(1, validateCount);

            IValidator validator3 = validator1.WithExtraValidation(() => true).WithSideEffect(new ApplyOnlyWhenValidSideEffect(() => validateCount++));

            // Due to cascading validation and side effects calling Validate on validator3
            // will cause the Validated event on validator1, which will trigger Validated
            // on validator2, as well as the side effect on validator2.
            validator3.Validate();
            Assert.AreEqual(3, validateCount); // Incremented by 2.

            // Turn off validator2 side effect by disabling cascading validation.
            validator2.ForwardEvents = false;

            validator3.Validate();
            Assert.AreEqual(4, validateCount); // Incremented by 1;

            // Override.
            validator1.IsValid = false; // Raises Validated event.

            validator3.Validate();
            Assert.AreEqual(4, validateCount); // Incremented by 0.
        }

        sealed class ApplyOnlyWhenValidSideEffect : ISideEffect
        {
            private readonly Action ApplyOnlyWhenValid;

            internal ApplyOnlyWhenValidSideEffect(Action applyOnlyWhenValid)
            {
                ApplyOnlyWhenValid = applyOnlyWhenValid;
            }

            public void Apply(bool isValid)
            {
                if (isValid) {
                    ApplyOnlyWhenValid();
                }
            }
        }
    }
}