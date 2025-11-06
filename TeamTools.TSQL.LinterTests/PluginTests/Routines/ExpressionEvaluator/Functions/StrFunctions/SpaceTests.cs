using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(Space))]
    public sealed class SpaceTests : BaseMockFunctionTest
    {
        private Space func;
        private SqlValue spaceCount;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Space();
            spaceCount = MakeInt(5);
        }

        [Test]
        public void Test_Space_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Space_ReturnsNullOnNegativeInput()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeInt(-1)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Space_ReturnsEmptyStringOnZeroCount()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeInt(0)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo(""));
        }

        [Test]
        public void Test_Space_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(spaceCount), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("     "));
        }
    }
}
