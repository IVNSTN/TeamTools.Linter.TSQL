using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Right))]
    public sealed class RightTests : BaseSubstringTest<Right>
    {
        [Test]
        public void Test_Right_ReturnsExpectedSubstring()
        {
            Test_Func_ReturnsExpectedString("QWERTY", 2, "TY");
        }

        [Test]
        public void Test_Right_ReturnsNullValueIfArgIsNull()
        {
            Test_Func_ReturnsNullValueIfEitherOfArgsIsNull();
        }
    }
}
