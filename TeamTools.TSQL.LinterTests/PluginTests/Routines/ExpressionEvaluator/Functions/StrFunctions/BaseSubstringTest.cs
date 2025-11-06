using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    public abstract class BaseSubstringTest<TFunc> : BaseMockFunctionTest
    where TFunc : SqlFunctionHandler, new()
    {
        private TFunc func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            func = new TFunc();
        }

        protected void Test_Func_ReturnsExpectedString_WithStart(string src, int start, int length, string dst)
        {
            var str = Factory.NewLiteral("dbo.VARCHAR", src, default);
            var pos = Factory.NewLiteral("dbo.INT", start.ToString(), default);
            var len = Factory.NewLiteral("dbo.INT", length.ToString(), default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, pos, len), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());

            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo(dst));
        }

        protected void Test_Func_ReturnsExpectedString(string src, int length, string dst)
        {
            var str = Factory.NewLiteral("dbo.VARCHAR", src, default);
            var len = Factory.NewLiteral("dbo.INT", length.ToString(), default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, len), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());

            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo(dst));
        }

        protected void Test_Func_ReturnsNullValueIfEitherOfArgsIsNull_WithStart()
        {
            var str = Factory.NewNull(default);
            var len = Factory.NewNull(default);
            var start = Factory.NewNull(default);

            // str is null
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, start, len), Context);
            Assert.That(res?.IsNull, Is.True);

            // start is null
            res = func.Evaluate(ArgFactory.MakeListOfValues(str, start, len), Context);
            Assert.That(res?.IsNull, Is.True);

            // len is null
            res = func.Evaluate(ArgFactory.MakeListOfValues(str, start, len), Context);
            Assert.That(res?.IsNull, Is.True);
        }

        protected void Test_Func_ReturnsNullValueIfEitherOfArgsIsNull()
        {
            var str = Factory.NewNull(default);
            var len = Factory.NewLiteral("dbo.INT", 123.ToString(), default);

            // str is null
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, len), Context);
            Assert.That(res?.IsNull, Is.True);

            str = Factory.NewLiteral("dbo.VARCHAR", "adsf", default);
            len = Factory.NewNull(default);

            // len is null
            res = func.Evaluate(ArgFactory.MakeListOfValues(str, len), Context);
            Assert.That(res?.IsNull, Is.True);
        }
    }
}
