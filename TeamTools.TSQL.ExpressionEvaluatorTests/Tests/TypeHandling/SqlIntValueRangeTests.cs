using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator.TypeHandling
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlIntValueRange))]
    public sealed class SqlIntValueRangeTests
    {
        [Test]
        public void Test_SqlIntValueRange_ConstructorSavesValues()
        {
            var range = new SqlIntValueRange(-111, 200);
            Assert.That(range.Low, Is.EqualTo(-111));
            Assert.That(range.High, Is.EqualTo(200));

            range = new SqlIntValueRange(0, 1);
            Assert.That(range.Low, Is.EqualTo(0));
            Assert.That(range.High, Is.EqualTo(1));
        }

        [Test]
        public void Test_SqlIntValueRange_ReversesRangeCorrectly()
        {
            var range = SqlIntValueRange.RevertRange(new SqlIntValueRange(-111, 200));

            Assert.That(range.Low, Is.EqualTo(-200));
            Assert.That(range.High, Is.EqualTo(111));

            range = SqlIntValueRange.RevertRange(new SqlIntValueRange(0, 1));

            Assert.That(range.Low, Is.EqualTo(-1));
            Assert.That(range.High, Is.EqualTo(0));
        }

        [Test]
        public void Test_SqlIntValueRange_CompareRespectsBothRangeBounds()
        {
            // second is wider
            var a = new SqlIntValueRange(5, 10);
            var b = new SqlIntValueRange(1, 20);

            Assert.That(SqlIntValueRange.Compare(a, b), Is.EqualTo(-1));
            Assert.That(SqlIntValueRange.Compare(b, a), Is.EqualTo(1));

            // equal
            a = new SqlIntValueRange(-10, 20);
            b = new SqlIntValueRange(-10, 20);

            Assert.That(SqlIntValueRange.Compare(a, b), Is.EqualTo(0));
            Assert.That(SqlIntValueRange.Compare(b, a), Is.EqualTo(0));

            // second has higher high bound
            a = new SqlIntValueRange(0, 20);
            b = new SqlIntValueRange(20, 21);

            Assert.That(SqlIntValueRange.Compare(a, b), Is.EqualTo(-1));
            Assert.That(SqlIntValueRange.Compare(b, a), Is.EqualTo(1));
        }

        // values copy-pasted from the method above
        [Test]
        public void Test_SqlIntValueRange_CompareToRespectsBothRangeBounds()
        {
            // second is wider
            var a = new SqlIntValueRange(5, 10);
            var b = new SqlIntValueRange(1, 20);

            Assert.That(a.CompareTo(b), Is.EqualTo(-1));
            Assert.That(b.CompareTo(a), Is.EqualTo(1));

            // equal
            a = new SqlIntValueRange(-10, 20);
            b = new SqlIntValueRange(-10, 20);

            Assert.That(a.CompareTo(b), Is.EqualTo(0));
            Assert.That(b.CompareTo(a), Is.EqualTo(0));

            // second has higher high bound
            a = new SqlIntValueRange(0, 20);
            b = new SqlIntValueRange(20, 21);

            Assert.That(a.CompareTo(b), Is.EqualTo(-1));
            Assert.That(b.CompareTo(a), Is.EqualTo(1));
        }
    }
}
