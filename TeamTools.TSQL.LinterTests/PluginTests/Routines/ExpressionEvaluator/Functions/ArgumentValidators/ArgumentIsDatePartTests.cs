using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.BuiltInFunctions
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(ArgumentIsDatePart))]
    public sealed class ArgumentIsDatePartTests : BaseArgValidationTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void Test_ArgumentIsDatePart_DoesNotFailOnNull()
        {
            var argData = new ArgumentValidation("MyArg", null, Context);

            AssertNotValid(() => ArgumentIsDatePart.Validate(argData, null));
        }

        [Test]
        public void Test_ArgumentIsDatePart_PassesValueOnSuccess()
        {
            var dtp = new DatePartArgument("MONTH");
            var argData = new ArgumentValidation("MyArg", dtp, Context);
            DatePartArgument res = null;

            Assert.That(ArgumentIsDatePart.Validate(argData, v => res = v), Is.True);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.DatePartValue, Is.EqualTo(DatePartEnum.Month));
            Assert.That(Violations.ViolationCount, Is.EqualTo(0));
        }

        [Test]
        public void Test_ArgumentIsDatePart_RegistersViolationIfUnknownProvided()
        {
            var dtp = new DatePartArgument("DUMMY");
            var argData = new ArgumentValidation("MyArg", dtp, Context);
            DatePartArgument res = null;

            Assert.That(ArgumentIsDatePart.Validate(argData, v => res = v), Is.False);
            Assert.That(res, Is.Null);
            Assert.That(dtp.DatePartValue, Is.EqualTo(DatePartEnum.Unknown));
            Assert.That(Violations.Violations.OfType<InvalidArgumentViolation>().Count(), Is.EqualTo(1));
        }
    }
}
