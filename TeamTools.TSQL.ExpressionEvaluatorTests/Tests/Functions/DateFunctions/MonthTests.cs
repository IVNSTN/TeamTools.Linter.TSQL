using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Month))]
    public sealed class MonthTests : BaseMockFunctionTest
    {
        private Month func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Month();
        }

        [Test]
        public void Test_Month_ReturnsApproximateRange()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeDateTime(null)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(1));
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(12));
        }

        [Test]
        public void Test_Month_ReturnsSpecificValueIfSourceIsPrecise()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeDateTime("2010-05-31")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(5));
        }
    }
}
