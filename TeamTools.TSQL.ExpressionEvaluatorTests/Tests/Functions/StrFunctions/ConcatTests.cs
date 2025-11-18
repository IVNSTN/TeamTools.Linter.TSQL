using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Concat))]
    public sealed class ConcatTests : BaseMockFunctionTest
    {
        private Concat func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Concat();
        }

        [Test]
        public void Test_Concat_ReturnsEmptyStringOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.Empty);
        }

        [Test]
        public void Test_Concat_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(
                ArgFactory.MakeListOfValues(
                    MakeStr("A"),
                    MakeStr("B"),
                    MakeStr("CDEF")),
                Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("ABCDEF"));
        }
    }
}
