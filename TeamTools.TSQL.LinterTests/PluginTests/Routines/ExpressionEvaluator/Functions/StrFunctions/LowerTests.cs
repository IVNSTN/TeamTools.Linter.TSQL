using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
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
