using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(Month))]
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
