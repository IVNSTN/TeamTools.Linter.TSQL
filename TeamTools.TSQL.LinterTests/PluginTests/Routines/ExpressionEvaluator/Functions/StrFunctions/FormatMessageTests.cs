using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(FormatMessage))]
    public sealed class FormatMessageTests : BaseMockFunctionTest
    {
        private FormatMessage func;
        private SqlValue template;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new FormatMessage();
            template = MakeStr("ABC %s %d DEF");
        }

        [Test]
        public void Test_FormatMessage_ReturnsNullOnNullTemplate()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), MakeStr("foo")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_FormatMessage_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(template, MakeStr("XXX"), MakeStr("123")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("ABC XXX 123 DEF"));
        }

        [Test]
        public void Test_FormatMessage_RegistersViolationIfNoWildcardsInTemplate()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("foo"), MakeStr("bar")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1), "no wildcards");
        }

        [Test]
        public void Test_FormatMessage_RegistersViolationIfNoArgsProvided()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(template), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1), "lack of args");
        }

        [Test]
        public void Test_FormatMessage_RegistersViolationWildcardCountMismatchesArgCount()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(template, MakeStr("foo")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(Violations.Violations.OfType<InvalidNumberOfArgumentsViolation>().Count(), Is.EqualTo(1), "lack of args");

            res = func.Evaluate(ArgFactory.MakeListOfValues(template, MakeStr("foo"), MakeStr("bar"), MakeStr("far")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(Violations.Violations.OfType<InvalidNumberOfArgumentsViolation>().Count(), Is.EqualTo(2), "extra args");
        }
    }
}
