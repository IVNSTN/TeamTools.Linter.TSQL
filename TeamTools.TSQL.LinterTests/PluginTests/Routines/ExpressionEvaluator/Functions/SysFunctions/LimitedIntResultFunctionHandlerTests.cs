using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(LimitedIntResultFunctionHandler))]
    public sealed class LimitedIntResultFunctionHandlerTests : BaseMockFunctionTest
    {
        private MockFunc func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new MockFunc();
        }

        [Test]
        public void Test_LimitedIntResultFunction_ReturnsUnknownIntWithExpectedRange()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.TypeName, Is.EqualTo("dbo.INT"));
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(123));
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(321));
        }

        private sealed class MockFunc : LimitedIntResultFunctionHandler
        {
            public MockFunc() : base("MyFn", new SqlIntValueRange(123, 321))
            { }
        }
    }
}
