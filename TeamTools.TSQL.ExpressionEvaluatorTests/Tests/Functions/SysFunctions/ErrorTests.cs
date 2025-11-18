using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Error))]
    public sealed class ErrorTests : BaseZeroArgFunctionTest<Error>
    {
        [Test]
        public void Test_Error_DoesNotAcceptArgs()
        {
            Test_Func_DoesNotAcceptArgs();
        }

        [Test]
        public void Test_Error_ReturnsUnknownInt()
        {
            Test_Func_ReturnsExpectedValue(res => Assert.That(res.TypeName, Is.EqualTo("INT")));
        }
    }
}
