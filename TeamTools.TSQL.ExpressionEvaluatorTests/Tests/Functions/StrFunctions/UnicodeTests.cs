using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Unicode))]
    public sealed class UnicodeTests : BaseMockFunctionTest
    {
        private Unicode func;
        private SqlStrTypeValue str;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Unicode();
            str = MakeStr("Ё");
        }

        [Test]
        public void Test_Unicode_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Unicode_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(1025));
        }

        [Test]
        public void Test_Unicode_ReturnsNullForEmptyInput()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str.ChangeTo("", default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }
    }
}
