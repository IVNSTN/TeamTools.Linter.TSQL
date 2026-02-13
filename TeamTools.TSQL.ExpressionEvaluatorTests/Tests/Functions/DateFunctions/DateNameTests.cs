using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(DateName))]
    public sealed class DateNameTests : BaseMockFunctionTest
    {
        private DateName func;
        private DatePartArgument datePart;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new DateName();
            datePart = new DatePartArgument("HOUR");
        }

        [Test]
        public void Test_DateName_ReturnsApproximateRange()
        {
            var res = func.Evaluate(ArgFactory.MakeList(datePart, new ValueArgument(MakeDateTime(null))), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            // Hour value 0-23 is 2 symbols max long
            Assert.That((res as SqlStrTypeValue).EstimatedSize, Is.EqualTo(2));
        }

        [Test]
        public void Test_DateName_ReturnsSpecificValue()
        {
            var res = func.Evaluate(ArgFactory.MakeList(datePart, new ValueArgument(MakeDateTime("2010-05-31 12:30:00"))), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("12"));
        }
    }
}
