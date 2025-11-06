using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
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
