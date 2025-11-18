using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;

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
    }
}
