using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.Functions.DateFunctions
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(DateTimeFromParts))]
    public class DateTimeFromPartsTests : BaseMockFunctionTest
    {
        private DateTimeFromParts func;
        private List<SqlFunctionArgument> funcArgs;
        private SqlDateTimeValue dt;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new DateTimeFromParts();
            funcArgs = ArgFactory.MakeListOfValues(
                MakeInt(2010),
                MakeInt(7),
                MakeInt(22),
                MakeInt(12),
                MakeInt(30),
                MakeInt(55),
                MakeInt(777));
        }

        [Test]
        public void Test_DateTimeFromParts_ReturnsPreciseValue()
        {
            var res = func.Evaluate(funcArgs, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlDateTimeValue>());

            var result = ((SqlDateTimeValue)res).Value;
            Assert.That(result.Year, Is.EqualTo(2010));
            Assert.That(result.Month, Is.EqualTo(7));
            Assert.That(result.Day, Is.EqualTo(22));
            Assert.That(result.Hour, Is.EqualTo(12));
            Assert.That(result.Minute, Is.EqualTo(30));
            Assert.That(result.Second, Is.EqualTo(55));
        }
    }
}
