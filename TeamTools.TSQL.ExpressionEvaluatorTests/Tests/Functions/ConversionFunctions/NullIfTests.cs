using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(NullIf))]
    public sealed class NullIfTests : BaseMockFunctionTest
    {
        private NullIf func;
        private SqlValue firstArg;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new NullIf();
            firstArg = MakeStr("A");
        }

        [Test]
        public void Test_NullIf_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), MakeStr("B")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "first null");
            Assert.That(firstArg.TypeName, Is.EqualTo(res.TypeName));

            res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "both null");
            Assert.That(firstArg.TypeName, Is.EqualTo(res.TypeName));
        }

        [Test]
        public void Test_NullIf_ReturnsFirstArgWhenStringsAreNotEqual()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(firstArg, MakeStr("B")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.EqualTo(firstArg));
        }

        [Test]
        public void Test_NullIf_ReturnsFirstArgWhenNumbersAreNotEqual()
        {
            var firstInt = MakeInt(-3);
            var res = func.Evaluate(ArgFactory.MakeListOfValues(firstInt, MakeInt(2)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.EqualTo(firstInt));
        }

        [Test]
        public void Test_NullIf_ReturnsNullWhenStringsAreEqual()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(firstArg, MakeStr("A")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
            Assert.That(res.TypeName, Is.EqualTo(firstArg.TypeName));
        }

        [Test]
        public void Test_NullIf_ReturnsNullWhenNumbersAreEqual()
        {
            var firstInt = MakeInt(123);
            var res = func.Evaluate(ArgFactory.MakeListOfValues(firstInt, MakeInt(123)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
            Assert.That(res.TypeName, Is.EqualTo(firstInt.TypeName));
        }

        [Test]
        public void Test_NullIf_ReturnsFirstArgWhenSecondIntIsBigger()
        {
            var firstInt = MakeInt(0).ChangeTo(new SqlIntValueRange(1, 100), default);
            var res = func.Evaluate(
                ArgFactory.MakeListOfValues(firstInt, MakeInt(5000)),
                Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res.TypeName, Is.EqualTo(firstInt.TypeName));
            Assert.That((res as SqlIntTypeValue).EstimatedSize.Low, Is.EqualTo(firstInt.EstimatedSize.Low));
            Assert.That((res as SqlIntTypeValue).EstimatedSize.High, Is.EqualTo(firstInt.EstimatedSize.High));
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test_NullIf_ReturnsFirstArgWhenSecondStringIsLonger()
        {
            var firstStr = MakeStr("dummmy").ChangeTo(3, default);
            var res = func.Evaluate(
                ArgFactory.MakeListOfValues(firstStr, MakeStr("ABCDEF")),
                Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res.TypeName, Is.EqualTo(firstStr.TypeName));
            Assert.That((res as SqlStrTypeValue).EstimatedSize, Is.EqualTo(firstStr.EstimatedSize));
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }
    }
}
