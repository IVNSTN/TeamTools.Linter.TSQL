using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(CursorStatus))]
    public sealed class CursorStatusTests : BaseMockFunctionTest
    {
        private CursorStatus func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new CursorStatus();
        }

        [Test]
        public void Test_CursorStatus_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), MakeStr("cr")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False, "null scope");
            Assert.That(Violations.ViolationCount, Is.EqualTo(1));

            res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("local"), Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False, "null name");
            Assert.That(Violations.ViolationCount, Is.EqualTo(2));
        }

        [Test]
        public void Test_CursorStatus_BadScopeIsReported()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("unknown"), MakeStr("cursor_name")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(Violations.ViolationCount, Is.EqualTo(1));
        }

        [Test]
        public void Test_CursorStatus_GoodScopeIsNotReported()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("local"), MakeStr("cursor_name")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(Violations.ViolationCount, Is.Zero);
        }
    }
}
