using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.Decimal
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlDecimalValueRange))]
    public class SqlDecimalValueRangeTests : BaseSqlTypeHandlerTestClass
    {
        private SqlDecimalTypeHandler typeHandler;

        private SqlDecimalTypeValueFactory ValueFactory => typeHandler.DecimalValueFactory;

        [OneTimeSetUp]
        public override void SetUp()
        {
            base.SetUp();
            typeHandler = new SqlDecimalTypeHandler(Converter, Violations);
        }

        [Test]
        public void Test_SqlDecimalValueRange_EqualityRespectsPrecisionAndScale()
        {
            var a = new SqlDecimalValueRange(0, 100, 18, 3);
            var b = new SqlDecimalValueRange(0, 100, 18, 3);

            // all the same
            Assert.That(a.Equals(b), Is.True);

            // something is different
            b = new SqlDecimalValueRange(0, 100, 18, 10);
            Assert.That(a.Equals(b), Is.False);

            b = new SqlDecimalValueRange(0, 100, 12, 3);
            Assert.That(a.Equals(b), Is.False);

            b = new SqlDecimalValueRange(100, 100, 18, 3);
            Assert.That(a.Equals(b), Is.False);

            b = new SqlDecimalValueRange(0, 0, 18, 3);
            Assert.That(a.Equals(b), Is.False);

            b = new SqlDecimalValueRange(1000, 10000, 18, 3);
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void Test_SqlDecimalValueRange_RevertRange()
        {
            var a = new SqlDecimalValueRange(99, 100, 18, 3);
            var b = SqlDecimalValueRange.RevertRange(a);

            Assert.That(b, Is.Not.Null);
            Assert.That(b.Low, Is.EqualTo(-100));
            Assert.That(b.High, Is.EqualTo(-99));

            // nothing happened to precision and scale
            Assert.That(b.Precision, Is.EqualTo(18));
            Assert.That(b.Scale, Is.EqualTo(3));
        }

        [TestCase(18, 9)]
        [TestCase(22, 13)]
        [TestCase(38, 17)]
        public void Test_SqlDecimalTypeReference_GetBytesRespectsPrecision(int precision, int bytes)
        {
            var typeRef = new SqlDecimalTypeReference("DECIMAL", new SqlDecimalValueRange(0, 1, precision, 3), ValueFactory);
            Assert.That(typeRef.Bytes, Is.EqualTo(bytes), precision.ToString());
        }

        [Test]
        public void Test_SqlDecimalValueRange_FixesIllegalPrecision()
        {
            var range = new SqlDecimalValueRange(1, 2, 200, 0);

            Assert.That(range.Precision, Is.EqualTo(38));
            Assert.That(range.Scale, Is.EqualTo(0));
        }

        [Test]
        public void Test_SqlDecimalValueRange_FixesIllegalScale()
        {
            var range = new SqlDecimalValueRange(1, 2, 15, 38);

            Assert.That(range.Precision, Is.EqualTo(15));
            Assert.That(range.Scale, Is.EqualTo(14));

            range = new SqlDecimalValueRange(1, 2, 15, -1);

            Assert.That(range.Precision, Is.EqualTo(15));
            Assert.That(range.Scale, Is.EqualTo(0));
        }
    }
}
