using NUnit.Framework;
using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Trim))]
    public sealed class TrimTests : BaseMockFunctionTest
    {
        private Trim func;
        private SqlValue str;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Trim();
            str = MakeStr("   test   ");
        }

        [Test]
        public void Test_Trim_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "null value");

            res = func.Evaluate(ArgFactory.MakeListOfValues(str, Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "null chars");
        }

        [Test]
        public void Test_Trim_TrimsBothSides()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("test"));
        }

        [Test]
        public void Test_Trim_TrimsCharSet()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("221133A B C321"), MakeStr("123")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("A B C"));
        }

        [Test]
        public void Test_Trim_RegistersRedundantCallIfNothingToTrim()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("221133A B C321")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("221133A B C321"), "no spaces");

            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(1));

            res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo(""), "empty string");

            Assert.That(Violations.Violations.OfType<RedundantFunctionCallViolation>().Count(), Is.EqualTo(2));
        }
    }
}
