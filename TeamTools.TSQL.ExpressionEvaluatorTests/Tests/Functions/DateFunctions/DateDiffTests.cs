using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.Functions.DateFunctions
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(DateDiff))]
    public class DateDiffTests : BaseMockFunctionTest
    {
        private DateDiff func;
        private List<SqlFunctionArgument> funcArgs;
        private SqlDateTimeValue dt;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new DateDiff();
            funcArgs = ArgFactory.MakeList(new DatePartArgument("YEAR"), new ValueArgument(MakeDateTime("2010-04-02")), new ValueArgument(MakeDateTime("2022-11-09")));
        }

        [Test]
        public void Test_DateDiff_ComputesDiffInYears()
        {
            var res = func.Evaluate(funcArgs, Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());

            Assert.That(((SqlIntTypeValue)res).Value, Is.EqualTo(12));
        }
    }
}
