using NUnit.Framework;
using System;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public abstract class BaseZeroArgFunctionTest<TFunc> : BaseMockFunctionTest
    where TFunc : SqlFunctionHandler, new()
    {
        private TFunc func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            func = new TFunc();
        }

        protected void Test_Func_DoesNotAcceptArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeList(null, null), Context);

            Assert.That(res, Is.Null);
            Assert.That(Violations.Violations.OfType<InvalidNumberOfArgumentsViolation>().Count(), Is.EqualTo(1));
        }

        protected void Test_Func_ReturnsExpectedValue(Action<SqlValue> callback)
        {
            var res = func.Evaluate(ArgFactory.MakeList(), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res.IsNull, Is.False);

            callback.Invoke(res);
        }
    }
}
