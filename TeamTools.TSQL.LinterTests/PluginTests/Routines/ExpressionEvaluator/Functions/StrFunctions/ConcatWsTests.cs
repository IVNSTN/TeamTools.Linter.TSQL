using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(ConcatWs))]
    public sealed class ConcatWsTests : BaseMockFunctionTest
    {
        private ConcatWs func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new ConcatWs();
        }

        [Test]
        public void Test_ConcatWs_ReturnsEmptyStringOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), Factory.NewNull(default), Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.Empty);
        }

        [Test]
        public void Test_ConcatWs_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(
                ArgFactory.MakeListOfValues(
                    MakeStr("-"),
                    MakeStr("A"),
                    MakeStr("B"),
                    MakeStr("CDEF")),
                Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("A-B-CDEF"));
        }
    }
}
