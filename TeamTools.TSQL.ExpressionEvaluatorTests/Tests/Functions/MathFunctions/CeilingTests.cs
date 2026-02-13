using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.MathFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Round))]
    public sealed class CeilingTests : BaseMockFunctionTest
    {
        private Ceiling func;
        private SqlDecimalTypeValue value;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Ceiling();
            value = MakeDecimal(123.51629m);
        }

        [Test]
        public void Test_Ceiling_ForDecimal()
        {
            var result = func.Evaluate(ArgFactory.MakeListOfValues(value), Context);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlDecimalTypeValue>());

            var number = (SqlDecimalTypeValue)result;
            Assert.That(number.IsPreciseValue, Is.True);
            Assert.That(number.Value, Is.EqualTo(124m));
        }

        [Test]
        public void Test_Ceiling_ForInt()
        {
            var result = func.Evaluate(ArgFactory.MakeListOfValues(MakeInt(123)), Context);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlDecimalTypeValue>());

            var number = (SqlDecimalTypeValue)result;
            Assert.That(number.IsPreciseValue, Is.True);
            Assert.That(number.Value, Is.EqualTo(123));
        }
    }
}
