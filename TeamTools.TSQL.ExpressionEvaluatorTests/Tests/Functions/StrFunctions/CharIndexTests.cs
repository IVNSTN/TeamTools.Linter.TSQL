using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(CharIndex))]
    public sealed class CharIndexTests : BaseMockFunctionTest
    {
        private CharIndex func;
        private SqlValue str;
        private SqlValue chr;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new CharIndex();
            str = Factory.NewLiteral("VARCHAR", "qwe;rty12;3", default);
            chr = Factory.NewLiteral("VARCHAR", ";", default);
        }

        [Test]
        public void Test_CharIndex_ReturnsPreciseIndexIfAllIsPrecise()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(chr, str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());

            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(4));
        }

        [Test]
        public void Test_CharIndex_ReturnsZeroIfNotFound()
        {
            var anotherChar = Factory.NewLiteral("VARCHAR", "=", default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(anotherChar, str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());

            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(0));
        }

        [Test]
        public void Test_CharIndex_ReturnsNullIfArgsAreNull()
        {
            // str is null
            var nullStr = Factory.NewNull(default);
            var res = func.Evaluate(ArgFactory.MakeListOfValues(chr, nullStr), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);

            // char is null
            var nullChar = Factory.NewNull(default);
            res = func.Evaluate(ArgFactory.MakeListOfValues(nullChar, str), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);

            // both null
            res = func.Evaluate(ArgFactory.MakeListOfValues(nullChar, nullStr), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_CharIndex_RespectsStartPos()
        {
            var pos = Factory.NewLiteral("INT", "5", default);

            var res = func.Evaluate(ArgFactory.MakeListOfValues(chr, str, pos), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());

            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(10));
        }
    }
}
