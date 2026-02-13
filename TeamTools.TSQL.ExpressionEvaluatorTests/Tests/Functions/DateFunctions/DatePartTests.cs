using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(DatePart))]
    public sealed class DatePartTests : BaseMockFunctionTest
    {
        private DatePart func;
        private SqlFunctionArgument datePart;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new DatePart();
            datePart = new DatePartArgument("DAY");
        }

        [Test]
        public void Test_DatePart_ReturnsApproximateRange()
        {
            var res = func.Evaluate(ArgFactory.MakeList(datePart, new ValueArgument(MakeDateTime(null))), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());

            var intResRange = ((SqlIntTypeValue)res).EstimatedSize;
            Assert.That(intResRange.Low, Is.EqualTo(1));
            Assert.That(intResRange.High, Is.EqualTo(31));
        }

        [Test]
        public void Test_DatePart_ReturnsPreciseAnswer()
        {
            var res = func.Evaluate(ArgFactory.MakeList(datePart, new ValueArgument(MakeDateTime("2025-11-06"))), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());

            var intRes = ((SqlIntTypeValue)res).Value;
            Assert.That(intRes, Is.EqualTo(6));
        }
    }
}
