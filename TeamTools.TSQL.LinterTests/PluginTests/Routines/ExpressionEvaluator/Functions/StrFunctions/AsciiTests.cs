using NUnit.Framework;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOfRule(typeof(Ascii))]
    public sealed class AsciiTests : BaseMockFunctionTest
    {
        private Ascii func;
        private SqlStrTypeValue str;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Ascii();
            str = MakeStr("A");
        }

        [Test]
        public void Test_Ascii_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Ascii_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(65));
        }

        [Test]
        public void Test_Ascii_ReturnsNullForEmptyInput()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str.ChangeTo("", default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Ascii_ReturnsByteIntEvenForUnicode()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str.ChangeTo("🎲", default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That((res as SqlIntTypeValue).Value, Is.LessThan(256));
        }
    }
}
