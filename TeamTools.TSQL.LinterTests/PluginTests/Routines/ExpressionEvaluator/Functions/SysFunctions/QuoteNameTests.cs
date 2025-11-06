using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(QuoteName))]
    public sealed class QuoteNameTests : BaseMockFunctionTest
    {
        private QuoteName func;
        private SqlValue str;
        private SqlValue chr;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new QuoteName();
            str = MakeStr("te}s{t");
            chr = MakeStr("{");
        }

        [Test]
        public void Test_QuoteName_AddsOuterBrackets()
        {
            var simpleStr = MakeStr("test");
            var simpleChr = MakeStr("(");
            var res = func.Evaluate(ArgFactory.MakeListOfValues(simpleStr, simpleChr), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("(test)"));
        }

        [Test]
        public void Test_QuoteName_EscapesInnerBrackets()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, chr), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("{te}}s{t}"));
        }

        [Test]
        public void Test_QuoteName_UsesSquareBracketsIfNoneProvided()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("[te}s{t]"), "when param omitted");

            var emptyChar = MakeStr("");
            res = func.Evaluate(ArgFactory.MakeListOfValues(str, emptyChar), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("[te}s{t]"), "when empty quote char");
        }

        [Test]
        public void Test_QuoteName_ReturnsNullOnNullInput()
        {
            var nullStr = Factory.NewNull(default);
            var nullChr = Factory.NewNull(default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, nullChr), Context);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "chr was null");

            res = func.Evaluate(ArgFactory.MakeListOfValues(nullStr, chr), Context);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "str was null");

            res = func.Evaluate(ArgFactory.MakeListOfValues(nullStr, nullChr), Context);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "both args were null");
        }

        [Test]
        public void Test_QuoteName_ReturnsNullForUnsupportedQuotationChar()
        {
            var anotherChar = MakeStr("+");
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, anotherChar), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_QuoteName_RegistersViolationForTooLongSourceString()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr(new string('x', 1000))), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
            Assert.That(Violations.Violations.OfType<ArgumentOutOfRangeViolation>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Test_QuoteName_RegistersViolationForEmptySourceString()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("[]"));
            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));
        }
    }
}
