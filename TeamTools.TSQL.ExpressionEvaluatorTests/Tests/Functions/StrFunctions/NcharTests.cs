using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Nchar))]
    public sealed class NcharTests : BaseMockFunctionTest
    {
        private Nchar func;
        private SqlValue code;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Nchar();
            code = MakeInt(65);
        }

        [Test]
        public void Test_Nchar_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Nchar_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(code), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res.TypeName, Is.EqualTo("NCHAR"));
            Assert.That(res, Is.InstanceOf<SqlStrTypeValue>());
            Assert.That((res as SqlStrTypeValue).Value, Is.EqualTo("A"));
        }
    }
}
