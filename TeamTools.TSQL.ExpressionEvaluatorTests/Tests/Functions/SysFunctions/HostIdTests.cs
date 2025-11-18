using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(HostId))]
    public sealed class HostIdTests : BaseZeroArgFunctionTest<HostId>
    {
        [Test]
        public void Test_HostId_DoesNotAcceptArgs()
        {
            Test_Func_DoesNotAcceptArgs();
        }

        [Test]
        public void Test_HostId_ReturnsUnknownStr()
        {
            Test_Func_ReturnsExpectedValue(res =>
            {
                Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
                Assert.That((res as SqlStrTypeValue).EstimatedSize, Is.EqualTo(10));
            });
        }
    }
}
