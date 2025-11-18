using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(DateName))]
    public sealed class DateNameTests : BaseMockFunctionTest
    {
        private DateName func;
        private SqlValue dt;
        private DatePartArgument datePart;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new DateName();
            // TODO : support real dates
            dt = MakeStr("dummy");
            datePart = new DatePartArgument("HOUR");
        }

        [Test]
        public void Test_DatePart_ReturnsApproximateRange()
        {
            var res = func.Evaluate(ArgFactory.MakeList(datePart, new ValueArgument(dt)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            // Hour value 0-23 is 2 symbols max long
            Assert.That((res as SqlStrTypeValue).EstimatedSize, Is.EqualTo(2));
        }
    }
}
