using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Lower))]
    public sealed class LowerTests : BaseStrManipulationTest<Lower>
    {
        [Test]
        public void Test_Lower_ReturnsExpectedString()
        {
            Test_Func_ReturnsExpectedString("ABcd EF", "abcd ef");
        }

        [Test]
        public void Test_Lower_ReturnsNullValueIfArgIsNull()
        {
            Test_Func_ReturnsNullValueIfEitherOfArgsIsNull();
        }
    }
}
