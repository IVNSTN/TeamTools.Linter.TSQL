using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Format))]
    public sealed class FormatTests : BaseMockFunctionTest
    {
        private Format func;
        private SqlValue src;
        private SqlStrTypeValue fmt;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Format();
            src = MakeInt(7);
            fmt = MakeStr("dd-mm-yyyy");
        }

        [Test]
        public void Test_Format_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), fmt), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "null source");

            res = func.Evaluate(ArgFactory.MakeListOfValues(src, Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "null format");
        }

        [Test]
        public void Test_Format_RegistersViolationForUnsupportedFormat()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(src, MakeStr("Ё")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
            Assert.That(Violations.Violations.OfType<InvalidArgumentViolation>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test_Format_DoesNotAllowStringsInSourceValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr(""), fmt), Context);

            Assert.That(res, Is.Null);
            Assert.That(Violations.Violations.OfType<InvalidArgumentViolation>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test_Format_ReturnsExpectedLength()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(src, fmt), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.False);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).EstimatedSize, Is.EqualTo(fmt.Value.Length));
        }

        [Test]
        public void Test_Format_ReturnsNullForUnsupportedType()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(default, MakeStr("d"), MakeStr("en-us")), Context);

            Assert.That(res, Is.Null);
        }
    }
}
