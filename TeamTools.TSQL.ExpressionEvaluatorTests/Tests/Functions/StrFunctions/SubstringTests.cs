using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Substring))]
    public sealed class SubstringTests : BaseMockFunctionTest
    {
        private Substring func;
        private SqlValue str;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Substring();
            str = MakeStr("ABCDEF");
        }

        [Test]
        public void Test_Substring_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default), MakeInt(3), MakeInt(2)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "str is null");

            res = func.Evaluate(ArgFactory.MakeListOfValues(str, Factory.NewNull(default), MakeInt(2)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "start is null");

            res = func.Evaluate(ArgFactory.MakeListOfValues(str, MakeInt(3), Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True, "len is null");
        }

        [Test]
        public void Test_Substring_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, MakeInt(3), MakeInt(2)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("CD"));
        }

        [Test]
        public void Test_Substring_ReturnsSubstringTillEndIfLengthTooBig()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, MakeInt(3), MakeInt(200)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("CDEF"));
        }

        [Test]
        public void Test_Substring_ReturnsEmptyStringIfStartIsOutside()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(str, MakeInt(35), MakeInt(2)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo(""));
        }
    }
}
