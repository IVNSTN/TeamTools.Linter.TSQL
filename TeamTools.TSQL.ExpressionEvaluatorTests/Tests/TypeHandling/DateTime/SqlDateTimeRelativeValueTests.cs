using NUnit.Framework;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.TypeHandling.DateTime
{
    [Category("TeamTools.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(SqlDateTimeRelativeValue))]
    public class SqlDateTimeRelativeValueTests
    {
        [Test]
        public void Test_SqlDateTimeRelativeValue_ValueComparison()
        {
            var a = new SqlDateTimeRelativeValue(DateTimeRangeKind.Past, DateDetails.Full);
            var b = new SqlDateTimeRelativeValue(DateTimeRangeKind.Future, DateDetails.Full);
            var c = new SqlDateTimeRelativeValue(DateTimeRangeKind.CurrentMoment, DateDetails.Full);

            Assert.That(a.CompareTo(b), Is.EqualTo(-1));
            Assert.That(a.CompareTo(c), Is.EqualTo(-1));

            Assert.That(b.CompareTo(a), Is.EqualTo(1));
            Assert.That(b.CompareTo(c), Is.EqualTo(1));

            Assert.That(c.CompareTo(a), Is.EqualTo(1));
            Assert.That(c.CompareTo(b), Is.EqualTo(-1));

            Assert.That(a.CompareTo(a), Is.Zero);
            Assert.That(b.CompareTo(b), Is.Zero);
            Assert.That(c.CompareTo(c), Is.Zero);
        }
    }
}
