using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Stuff))]
    public sealed class StuffTests : BaseMockFunctionTest
    {
        private Stuff func;
        private SqlValue str;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Stuff();
            str = MakeStr("test");
        }

        [Test]
        public void Test_Stuff_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), MakeInt(1), MakeInt(2), MakeStr("asdf")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "src null");

            res = func.Evaluate(ArgFactory.MakeListOfValues(str, Factory.NewNull(default), MakeInt(2), MakeStr("asdf")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "start null");

            res = func.Evaluate(ArgFactory.MakeListOfValues(str, MakeInt(1), Factory.NewNull(default), MakeStr("asdf")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "len null");

            res = func.Evaluate(ArgFactory.MakeListOfValues(str, MakeInt(1), MakeInt(2), Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "substr null");
        }

        [Test]
        public void Test_Stuff_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, MakeInt(2), MakeInt(2), MakeStr("***")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("t***t"));
        }

        [Test]
        public void Test_Stuff_ApproximatesResultSize()
        {
            var res = func.Evaluate(
                ArgFactory.MakeListOfValues(
                    Factory.NewValue(new SqlStrTypeReference("VARCHAR", 10, Factory), SqlValueKind.Unknown),
                    Factory.NewValue(new SqlIntTypeReference("INT", new SqlIntValueRange(2, 3), Factory), SqlValueKind.Unknown),
                    Factory.NewValue(new SqlIntTypeReference("INT", new SqlIntValueRange(5, 6), Factory), SqlValueKind.Unknown),
                    Factory.NewValue(new SqlStrTypeReference("VARCHAR", 12, Factory), SqlValueKind.Unknown)),
                Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            // 10 - 6 + 12
            Assert.That((res as SqlStrTypeValue).EstimatedSize, Is.EqualTo(16));
        }
    }
}
