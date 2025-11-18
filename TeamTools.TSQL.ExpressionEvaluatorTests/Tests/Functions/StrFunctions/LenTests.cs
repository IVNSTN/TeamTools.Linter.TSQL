using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(Len))]
    public sealed class LenTests : BaseMockFunctionTest
    {
        private Len func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new Len();
        }

        [Test]
        public void Test_Len_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_Len_TrimsSpacesForVarchar()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("ABC     ")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(3));
        }

        [Test]
        public void Test_Len_KeepsSpacesForChar()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewLiteral("NCHAR", "ABC    ", default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(7));
        }

        [Test]
        public void Test_Len_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("ABCDE")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(5));
        }

        [Test]
        public void Test_Len_ReturnsSameBytesForNVarchar()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewLiteral("NVARCHAR", "ABCDE", default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(5));
        }
    }
}
