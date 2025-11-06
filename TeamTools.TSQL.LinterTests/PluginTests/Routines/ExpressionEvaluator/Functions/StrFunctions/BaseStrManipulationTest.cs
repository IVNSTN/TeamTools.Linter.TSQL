using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public abstract class BaseStrManipulationTest<TFunc> : BaseMockFunctionTest
    where TFunc : SqlFunctionHandler, new()
    {
        private TFunc func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            func = new TFunc();
        }

        protected void Test_Func_ReturnsExpectedString(string src, string dst)
        {
            var str = Factory.NewLiteral("dbo.VARCHAR", src, default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());

            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo(dst));
        }

        protected void Test_Func_ReturnsNullValueIfEitherOfArgsIsNull()
        {
            var str = Factory.NewNull(default);
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }
    }
}
