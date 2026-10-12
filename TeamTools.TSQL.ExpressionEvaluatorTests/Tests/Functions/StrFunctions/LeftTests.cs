using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Left))]
    public sealed class LeftTests : BaseSubstringTest<Left>
    {
        [Test]
        public void Test_Left_ReturnsExpectedSubstring()
        {
            Test_Func_ReturnsExpectedString("QWERTY", 2, "QW");
        }

        [Test]
        public void Test_Left_ReturnsNullValueIfEitherOfArgsIsNull()
        {
            Test_Func_ReturnsNullValueIfEitherOfArgsIsNull();
        }

        [Test]
        public void Test_Left_FailsOnNegativeStart()
        {
            var res = FuncInstance.Evaluate(ArgFactory.MakeListOfValues(MakeStr("asdf"), MakeInt(-1)), Context);

            Assert.That(Violations.Violations.OfType<InvalidArgumentViolation>().Count(), Is.EqualTo(1), "length must be positive");
            Assert.That(res, Is.Null);
        }
    }
}
