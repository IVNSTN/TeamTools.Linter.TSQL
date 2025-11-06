using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(Char))]
    public sealed class CharTests : BaseMockFunctionTest
    {
        private Char func;
        private SqlValue code;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Char();
            code = MakeInt(9);
        }

        [Test]
        public void Test_Char_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Char_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(code), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res.TypeName, Is.EqualTo("dbo.CHAR"));
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("\t"));
        }

        [Test]
        public void Test_Char_ReturnsNullForOutOfRangeSource()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeInt(-1)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "negative");

            res = func.Evaluate(ArgFactory.MakeListOfValues(MakeInt(256)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "256");

            res = func.Evaluate(ArgFactory.MakeListOfValues(MakeInt(123456)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "big integer");
        }
    }
}
