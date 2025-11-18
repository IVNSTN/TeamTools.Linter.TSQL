using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Month))]
    public sealed class MonthTests : BaseMockFunctionTest
    {
        private Month func;
        private SqlValue dt;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Month();
            // TODO : support real dates
            dt = MakeStr("dummy");
        }

        [Test]
        public void Test_Month_ReturnsApproximateRange()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(dt), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(1));
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(12));
        }
    }
}
