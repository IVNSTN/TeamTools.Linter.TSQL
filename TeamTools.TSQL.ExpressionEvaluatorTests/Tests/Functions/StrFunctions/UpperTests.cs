using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Upper))]
    public sealed class UpperTests : BaseStrManipulationTest<Upper>
    {
        [Test]
        public void Test_Upper_ReturnsExpectedString()
        {
            Test_Func_ReturnsExpectedString("ABcd EF", "ABCD EF");
        }

        [Test]
        public void Test_Upper_ReturnsNullValueIfArgIsNull()
        {
            Test_Func_ReturnsNullValueIfEitherOfArgsIsNull();
        }
    }
}
