using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(Reverse))]
    public sealed class ReverseTests : BaseStrManipulationTest<Reverse>
    {
        [Test]
        public void Test_Reverse_ReturnsExpectedString()
        {
            Test_Func_ReturnsExpectedString("ABcd EF", "FE dcBA");
        }

        [Test]
        public void Test_Reverse_ReturnsNullValueIfArgIsNull()
        {
            Test_Func_ReturnsNullValueIfEitherOfArgsIsNull();
        }
    }
}
