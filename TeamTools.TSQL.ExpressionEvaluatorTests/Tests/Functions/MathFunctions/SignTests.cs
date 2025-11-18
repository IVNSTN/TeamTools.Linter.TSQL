using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.MathFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Sign))]
    public sealed class SignTests : BaseMockFunctionTest
    {
        private Sign func;
        private SqlIntTypeValue value;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Sign();
            value = MakeInt(-3);
        }

        [Test]
        public void Test_Sign_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Sign_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(value), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(-1), "negative");

            value = value.ChangeTo(1000, default);

            res = func.Evaluate(ArgFactory.MakeListOfValues(value), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(1), "positive");

            value = value.ChangeTo(0, default);

            res = func.Evaluate(ArgFactory.MakeListOfValues(value), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(0), "zero");
        }

        [Test]
        public void Test_Sign_RegistersRedundantCallViolationForPreciseValue()
        {
            Test_Sign_ReturnsExpectedPreciseValue();

            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(3));
        }

        [Test]
        public void Test_Sign_ReturnsPresiceValueIfValueRangeIsClear()
        {
            var intValue = new SqlIntTypeValue(
                new SqlIntTypeHandler(Context.Converter, Context.Violations),
                new SqlIntTypeReference("INT", new SqlIntValueRange(100, 200), Factory),
                SqlValueKind.Unknown,
                default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(intValue), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(1), "positive");

            intValue = intValue.ChangeTo(new SqlIntValueRange(-300, -1), default);

            res = func.Evaluate(ArgFactory.MakeListOfValues(intValue), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(-1), "negative");
        }

        [Test]
        public void Test_Sign_LimitsRangeForApproximateValue()
        {
            var intValue = new SqlIntTypeValue(
                new SqlIntTypeHandler(Context.Converter, Context.Violations),
                new SqlIntTypeReference("INT", new SqlIntValueRange(0, 200), Factory),
                SqlValueKind.Unknown,
                default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(intValue), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(0), "positive");
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(1), "positive");

            intValue = intValue.ChangeTo(new SqlIntValueRange(-300, 0), default);

            res = func.Evaluate(ArgFactory.MakeListOfValues(intValue), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(-1), "negative");
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(0), "negative");
        }

        [Test]
        public void Test_Sign_ReturnsLimitedRangeForUnknownValue()
        {
            var res = func.Evaluate(ArgFactory.MakeList(new ValueArgument(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(-1));
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(1));
        }
    }
}
