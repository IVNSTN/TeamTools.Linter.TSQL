using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.MathFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(Abs))]
    public sealed class AbsTests : BaseMockFunctionTest
    {
        private Abs func;
        private SqlValue value;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Abs();
            value = MakeInt(-7);
        }

        [Test]
        public void Test_Abs_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Abs_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(value), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(7));
        }

        [Test]
        public void Test_Abs_RevertsRangeIfNegative()
        {
            var intValue = new SqlIntTypeValue(
                new SqlIntTypeHandler(Context.Converter, Context.Violations),
                new SqlIntTypeReference("dbo.INT", new SqlIntValueRange(-300, -1), Factory),
                SqlValueKind.Unknown,
                default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(intValue), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(1));
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(300));
        }

        [Test]
        public void Test_Abs_RegistersViolationIfSourceIsPositive()
        {
            var intValue = new SqlIntTypeValue(
                new SqlIntTypeHandler(Context.Converter, Context.Violations),
                new SqlIntTypeReference("dbo.INT", new SqlIntValueRange(900, 1000), Factory),
                SqlValueKind.Unknown,
                default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(intValue), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(900));
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(1000));

            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }
    }
}
