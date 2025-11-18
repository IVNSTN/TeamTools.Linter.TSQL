using NUnit.Framework;
using System.Numerics;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlBigIntValueRange))]
    public sealed class SqlBigIntValueRangeTests
    {
        [Test]
        public void Test_SqlBigIntValueRange_ConstructorSavesValues()
        {
            var range = new SqlBigIntValueRange(-111, 200);
            Assert.That(range.Low, Is.EqualTo((BigInteger)(-111)));
            Assert.That(range.High, Is.EqualTo((BigInteger)200));

            range = new SqlBigIntValueRange(0, 1);
            Assert.That(range.Low, Is.EqualTo((BigInteger)0));
            Assert.That(range.High, Is.EqualTo((BigInteger)1));
        }

        [Test]
        public void Test_SqlBigIntValueRange_ReversesRangeCorrectly()
        {
            var range = SqlBigIntValueRange.RevertRange(new SqlBigIntValueRange(-111, 200));

            Assert.That(range.Low, Is.EqualTo((BigInteger)(-200)));
            Assert.That(range.High, Is.EqualTo((BigInteger)111));

            range = SqlBigIntValueRange.RevertRange(new SqlBigIntValueRange(0, 1));

            Assert.That(range.Low, Is.EqualTo((BigInteger)(-1)));
            Assert.That(range.High, Is.EqualTo((BigInteger)0));
        }

        [Test]
        public void Test_SqlBigIntValueRange_CompareRespectsBothRangeBounds()
        {
            // second is wider
            var a = new SqlBigIntValueRange(5, 10);
            var b = new SqlBigIntValueRange(1, 20);

            Assert.That(SqlBigIntValueRange.Compare(a, b), Is.EqualTo(-1));
            Assert.That(SqlBigIntValueRange.Compare(b, a), Is.EqualTo(1));

            // equal
            a = new SqlBigIntValueRange(-10, 20);
            b = new SqlBigIntValueRange(-10, 20);

            Assert.That(SqlBigIntValueRange.Compare(a, b), Is.EqualTo(0));
            Assert.That(SqlBigIntValueRange.Compare(b, a), Is.EqualTo(0));

            // second has higher high bound
            a = new SqlBigIntValueRange(0, 20);
            b = new SqlBigIntValueRange(20, 21);

            Assert.That(SqlBigIntValueRange.Compare(a, b), Is.EqualTo(-1));
            Assert.That(SqlBigIntValueRange.Compare(b, a), Is.EqualTo(1));
        }

        // values copy-pasted from the method above
        [Test]
        public void Test_SqlBigIntValueRange_CompareToRespectsBothRangeBounds()
        {
            // second is wider
            var a = new SqlBigIntValueRange(5, 10);
            var b = new SqlBigIntValueRange(1, 20);

            Assert.That(a.CompareTo(b), Is.EqualTo(-1));
            Assert.That(b.CompareTo(a), Is.EqualTo(1));

            // equal
            a = new SqlBigIntValueRange(-10, 20);
            b = new SqlBigIntValueRange(-10, 20);

            Assert.That(a.CompareTo(b), Is.EqualTo(0));
            Assert.That(b.CompareTo(a), Is.EqualTo(0));

            // second has higher high bound
            a = new SqlBigIntValueRange(0, 20);
            b = new SqlBigIntValueRange(20, 21);

            Assert.That(a.CompareTo(b), Is.EqualTo(-1));
            Assert.That(b.CompareTo(a), Is.EqualTo(1));
        }
    }
}
