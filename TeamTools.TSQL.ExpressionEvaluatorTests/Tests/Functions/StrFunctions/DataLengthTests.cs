using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(DataLength))]
    public sealed class DataLengthTests : BaseMockFunctionTest
    {
        private DataLength func;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            func = new DataLength();
        }

        [Test]
        public void Test_DataLength_ReturnsNullOnNullArgs()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewNull(default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.True);
        }

        [Test]
        public void Test_DataLength_ReturnsExpectedPreciseValue()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(MakeStr("ABCDE")), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(5));
        }

        [Test]
        public void Test_DataLength_ReturnsDoubleBytesForNVarchar()
        {
            var res = func.Evaluate(ArgFactory.MakeListOfValues(Factory.NewLiteral("NVARCHAR", "ABCDE", default)), Context);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.IsNull, Is.False);
            Assert.That(res.IsPreciseValue, Is.True);
            Assert.That(res, Is.InstanceOf<SqlIntTypeValue>());
            Assert.That((res as SqlIntTypeValue).Value, Is.EqualTo(10));
        }
    }
}
