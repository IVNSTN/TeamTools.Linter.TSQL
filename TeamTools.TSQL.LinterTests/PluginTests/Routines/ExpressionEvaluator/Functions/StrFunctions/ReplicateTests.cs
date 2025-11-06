using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(Replicate))]
    public sealed class ReplicateTests : BaseMockFunctionTest
    {
        private Replicate func;
        private SqlValue str;
        private SqlValue count;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Replicate();
            str = MakeStr("test+");
            count = MakeInt(3);
        }

        [Test]
        public void Test_Replicate_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), count), Context);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "null str");

            res = func.Evaluate(ArgFactory.MakeListOfValues(str, Factory.NewNull(default)), Context);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "null count");

            res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), Factory.NewNull(default)), Context);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "both null");
        }

        [Test]
        public void Test_Replicate_ReturnsNullOnNegativeCount()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, MakeInt(-1)), Context);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Replicate_ReturnsEmptyStringOnZeroCount()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, MakeInt(0)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo(""));
        }

        [Test]
        public void Test_Replicate_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, count), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("test+test+test+"));
        }
    }
}
