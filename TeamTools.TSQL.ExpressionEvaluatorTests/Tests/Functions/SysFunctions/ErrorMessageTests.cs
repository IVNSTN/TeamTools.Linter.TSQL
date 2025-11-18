using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(ErrorMessage))]
    public sealed class ErrorMessageTests : BaseZeroArgFunctionTest<ErrorMessage>
    {
        [Test]
        public void Test_ErrorMessage_DoesNotAcceptArgs()
        {
            Test_Func_DoesNotAcceptArgs();
        }

        [Test]
        public void Test_ErrorMessage_ReturnsUnknownStr()
        {
            Test_Func_ReturnsExpectedValue(res =>
            {
                // TODO : check if it is unicode
                // maybe switch from mock type handler to real ones
                Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
                Assert.That((res as SqlStrTypeValue).EstimatedSize, Is.EqualTo(4000));
            });
        }
    }
}
