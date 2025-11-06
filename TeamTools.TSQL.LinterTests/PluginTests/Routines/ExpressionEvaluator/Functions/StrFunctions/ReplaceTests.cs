using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(Replace))]
    public sealed class ReplaceTests : BaseMockFunctionTest
    {
        private Replace func;
        private SqlValue str;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Replace();
            str = MakeStr("test");
        }

        [Test]
        public void Test_Replace_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), str, str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "str null");

            res = func.Evaluate(ArgFactory.MakeListOfValues(str, Factory.NewNull(default), str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "search null");

            res = func.Evaluate(ArgFactory.MakeListOfValues(str, str, Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "replacement null");
        }

        [Test]
        public void Test_Replace_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, MakeStr("t"), MakeStr("**")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("**es**"));
        }

        [Test]
        public void Test_Replace_ApproximatesResultSize()
        {
            var res = func.Evaluate(
                ArgFactory.MakeListOfValues(
                    Factory.NewValue(new SqlStrTypeReference("dbo.VARCHAR", 123, Factory), SqlValueKind.Unknown),
                    Factory.NewValue(new SqlStrTypeReference("dbo.VARCHAR", 1, Factory), SqlValueKind.Unknown),
                    Factory.NewValue(new SqlStrTypeReference("dbo.VARCHAR", 6, Factory), SqlValueKind.Unknown)),
                Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).EstimatedSize, Is.EqualTo(148));
        }
    }
}
