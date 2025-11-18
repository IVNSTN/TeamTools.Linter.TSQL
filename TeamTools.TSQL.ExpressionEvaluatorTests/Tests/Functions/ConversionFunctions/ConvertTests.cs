using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Convert))]
    public sealed class ConvertTests : BaseMockFunctionTest
    {
        private Convert func;
        private SqlValue str;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Convert();
            str = MakeStr("test");
            str.Source = new SqlValueSource(SqlValueSourceKind.Expression, default);
        }

        [Test]
        public void Test_Convert_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeList(new ValueArgument(Factory.NewNull(default)), new TypeArgument(str.TypeReference)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Convert_RegistersViolationForRedundantConversion()
        {
            var res = func.Evaluate(ArgFactory.MakeList(new ValueArgument(str), new TypeArgument(str.TypeReference)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(str, Is.EqualTo(res));
            Assert.That(Violations.Violations.OfType<RedundantTypeConversionViolation>().Count(), Is.EqualTo(1));
        }
    }
}
