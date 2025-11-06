using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
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
