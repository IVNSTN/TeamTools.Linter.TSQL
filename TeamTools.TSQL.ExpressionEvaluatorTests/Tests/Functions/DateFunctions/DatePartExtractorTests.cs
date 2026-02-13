using NUnit.Framework;
using System;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions;
using DatePart = TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto.DatePartEnum;

namespace TeamTools.TSQL.ExpressionEvaluatorTests.Tests.Functions.DateFunctions
{
    [Category("TeamTools.TSQL.ExpressionEvaluator.EvalFunctions")]
    [TestOf(typeof(DatePartExtractor))]
    internal class DatePartExtractorTests
    {
        private readonly DateTime dateValue = new System.DateTime(2005, 7, 31, 12, 30, 55, 779);

        [Test]
        public void Test_DatePartExtractor_ReturnsZeroOnZeroDate()
        {
            int datePart = 0;

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(DateTime.MinValue, DatePart.Year, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(0));

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(DateTime.MinValue, DatePart.Day, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(0));
        }

        [Test]
        public void Test_DatePartExtractor_SupportsDateParts()
        {
            int datePart = 0;

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(dateValue, DatePart.Year, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(2005));

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(dateValue, DatePart.Quarter, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(3));

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(dateValue, DatePart.Month, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(7));

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(dateValue, DatePart.Day, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(31));

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(dateValue, DatePart.DayOfYear, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(212));

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(dateValue, DatePart.Week, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(31));

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(dateValue, DatePart.Hour, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(12));

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(dateValue, DatePart.Minute, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(30));

            Assert.That(DatePartExtractor.ExtractDatePartFromSpecificDate(dateValue, DatePart.Second, out datePart), Is.True);
            Assert.That(datePart, Is.EqualTo(55));
        }
    }
}
