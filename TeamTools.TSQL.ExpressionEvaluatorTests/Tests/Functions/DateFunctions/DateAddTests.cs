using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.Functions.DateFunctions
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(DateAdd))]
    public class DateAddTests : BaseMockFunctionTest
    {
        private DateAdd func;
        private List<SqlFunctionArgument> funcArgs;
        private SqlDateTimeValue dt;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new DateAdd();
            funcArgs = ArgFactory.MakeList(new DatePartArgument("YEAR"), new ValueArgument(MakeInt(3)), new ValueArgument(MakeDateTime("2022-11-09")));
        }

        [Test]
        public void Test_DateDiff_ComputesDiffInYears()
        {
            var res = func.Evaluate(funcArgs, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlDateTimeValue>());

            Assert.That(((SqlDateTimeValue)res).Value.Year, Is.EqualTo(2025));
        }

        [Test]
        public void Test_DateDiff_ComputesDiffInDays()
        {
            var diffArgs = ArgFactory.MakeList(new DatePartArgument("DAY"), new ValueArgument(MakeInt(3)), new ValueArgument(MakeDateTime("2022-11-09")));
            var res = func.Evaluate(diffArgs, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlDateTimeValue>());

            Assert.That(((SqlDateTimeValue)res).Value.Day, Is.EqualTo(12));
        }
    }
}
