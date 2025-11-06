using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.BuiltInFunctions
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(ArgumentIsValidInt))]
    public sealed class ArgumentIsValidIntTests : BaseArgValidationTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void Test_ArgumentIsValidInt_DoesNotFailOnNull()
        {
            var argData = new ArgumentValidation("MyArg", null, Context);

            AssertNotValid(() => ArgumentIsValidInt.Validate(argData, null, null));
            Assert.DoesNotThrow(() => ArgumentIsValidInt.Validate(argData, IntValue, null));
        }

        [Test]
        public void Test_ArgumentIsValidInt_PassesValueOnSuccess()
        {
            var argData = new ArgumentValidation("MyArg", null, Context);
            SqlValue res = null;

            Assert.That(ArgumentIsValidInt.Validate(argData, IntValue, v => res = v), Is.True);
            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
        }

        [Test]
        public void Test_ArgumentIsValidInt_CanConvertStrToInt()
        {
            var argData = new ArgumentValidation("MyArg", null, Context);
            SqlValue res = null;

            Assert.That(ArgumentIsValidInt.Validate(argData, StrValue, v => res = v), Is.True);
            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
        }

        [Test]
        public void Test_ArgumentIsValidInt_ChecksValidRangeIfProvided()
        {
            var argData = new ArgumentValidation("MyArg", null, Context);
            SqlValue res = null;
            var validRange = new SqlIntValueRange(0, 1000);

            Assert.That(ArgumentIsValidInt.Validate(argData, StrValue, v => res = v, validRange), Is.True);
            Assert.That(res, Is.Not.Null);

            res = null;
            var invalidRange = new SqlIntValueRange(-1000, -100);

            Assert.That(ArgumentIsValidInt.Validate(argData, StrValue, v => res = v, invalidRange), Is.False);
            Assert.That(res, Is.Null);
        }
    }
}
