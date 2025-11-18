using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentValidators;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.BuiltInFunctions
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(ArgumentIsValidStr))]
    public sealed class ArgumentIsValidStrTests : BaseArgValidationTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [Test]
        public void Test_ArgumentIsValidStr_DoesNotFailOnNull()
        {
            var argData = new ArgumentValidation("MyArg", null, Context);

            AssertNotValid(() => ArgumentIsValidStr.Validate(argData, null, null));
            Assert.DoesNotThrow(() => ArgumentIsValidStr.Validate(argData, IntValue, null));
        }

        [Test]
        public void Test_ArgumentIsValidStr_PassesValueOnSuccess()
        {
            var argData = new ArgumentValidation("MyArg", null, Context);
            SqlValue res = null;

            Assert.That(ArgumentIsValidStr.Validate(argData, StrValue, v => res = v), Is.True);
            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
        }

        [Test]
        public void Test_ArgumentIsValidStr_CanConvertIntToStr()
        {
            var argData = new ArgumentValidation("MyArg", null, Context);
            SqlValue res = null;

            Assert.That(ArgumentIsValidStr.Validate(argData, IntValue, v => res = v), Is.True);
            Assert.That(res, Is.Not.Null);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
        }
    }
}
