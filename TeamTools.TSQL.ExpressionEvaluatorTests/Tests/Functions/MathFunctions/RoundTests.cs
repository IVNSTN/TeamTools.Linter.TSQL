using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.MathFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Round))]
    public sealed class RoundTests : BaseMockFunctionTest
    {
        private Round func;
        private SqlDecimalTypeValue value;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Round();
            value = MakeDecimal(123.51629m);
        }

        [Test]
        public void Test_Round_Fractions()
        {
            var result = func.Evaluate(ArgFactory.MakeListOfValues(value, MakeInt(-2)), Context);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlDecimalTypeValue>());

            var number = (SqlDecimalTypeValue)result;
            Assert.That(number.IsPreciseValue, Is.True);
            Assert.That(number.Value, Is.EqualTo(100m));
        }

        [Test]
        public void Test_Round_Natural()
        {
            var result = func.Evaluate(ArgFactory.MakeListOfValues(value, MakeInt(2)), Context);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlDecimalTypeValue>());

            var number = (SqlDecimalTypeValue)result;
            Assert.That(number.IsPreciseValue, Is.True);
            Assert.That(number.Value, Is.EqualTo(123.52m));
        }

        [Test]
        public void Test_Round_ForInt()
        {
            var result = func.Evaluate(ArgFactory.MakeListOfValues(MakeInt(100), MakeInt(2)), Context);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SqlDecimalTypeValue>());

            var number = (SqlDecimalTypeValue)result;
            Assert.That(number.IsPreciseValue, Is.True);
            Assert.That(number.Value, Is.EqualTo(100));
        }

        [Test]
        public void Test_Round_ForUnknownLength()
        {
            var result = func.Evaluate(ArgFactory.MakeListOfValues(value, TypeHandler.ValueFactory.NewValue(value.TypeReference, SqlValueKind.Unknown)), Context);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsPreciseValue, Is.False);
        }
    }
}
