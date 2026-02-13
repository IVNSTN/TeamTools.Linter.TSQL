using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.Functions.DateFunctions
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(EndOfMonth))]
    public sealed class EndOfMonthTests : BaseMockFunctionTest
    {
        private EndOfMonth func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new EndOfMonth();
        }

        [Test]
        public void Test_EndOfMonth_ReturnsApproximateRange()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeDateTime(null)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlDateOnlyValue>());
        }

        [Test]
        public void Test_EndOfMonth_ReturnsSpecificValueIfSourceIsPrecise()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeDateTime("2010-05-01")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlDateOnlyValue>());
            Assert.That((res as SqlDateOnlyValue).Value.Month, Is.EqualTo(5));
            Assert.That((res as SqlDateOnlyValue).Value.Day, Is.EqualTo(31));
        }

        [Test]
        public void Test_EndOfMonth_ReturnsRespectsLeapYear()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeDateTime("2000-02-22")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlDateOnlyValue>());
            Assert.That((res as SqlDateOnlyValue).Value.Month, Is.EqualTo(2));
            Assert.That((res as SqlDateOnlyValue).Value.Day, Is.EqualTo(29));
        }
    }
}
