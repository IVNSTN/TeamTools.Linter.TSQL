using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(DatePart))]
    public sealed class DatePartTests : BaseMockFunctionTest
    {
        private DatePart func;
        private SqlFunctionArgument datePart;
        private SqlValue dt;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new DatePart();
            // TODO : support real dates
            dt = MakeStr("dummy");
            datePart = new DatePartArgument("DAY");
        }

        [Test]
        public void Test_DatePart_ReturnsApproximateRange()
        {
            var res = func.Evaluate(ArgFactory.MakeList(datePart, new ValueArgument(dt)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(1));
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(31));
        }
    }
}
