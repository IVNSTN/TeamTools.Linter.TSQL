using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(XactState))]
    public sealed class XactStateTests : BaseZeroArgFunctionTest<XactState>
    {
        [Test]
        public void Test_XactState_DoesNotAcceptArgs()
        {
            Test_Func_DoesNotAcceptArgs();
        }

        [Test]
        public void Test_XactState_ReturnsUnknownLimitedInt()
        {
            Test_Func_ReturnsExpectedValue(res =>
            {
                Assert.That(res.TypeName, Is.EqualTo("SMALLINT"));
                Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(-1));
                Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(1));
            });
        }
    }
}
