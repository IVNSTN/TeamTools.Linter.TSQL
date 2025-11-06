using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
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
                Assert.That(res.TypeName, Is.EqualTo("dbo.SMALLINT"));
                Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(-1));
                Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(1));
            });
        }
    }
}
