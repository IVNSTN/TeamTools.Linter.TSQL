using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.DateFunctions;

namespace TeamTools.TSQL.LinterTests.Routines.ExpressionEvaluator
{
    [Category("Linter.TSQL.ExpressionEvaluator")]
    [TestOf(typeof(DatePartToRangeConverter))]
    public sealed class DatePartToRangeConverterTests
    {
        private static readonly IList<DatePartEnum> EnumValues = Enum.GetValues(typeof(DatePartEnum)).Cast<DatePartEnum>().ToList();

        [Test]
        public void Test_DatePartToRangeConverter_GivesIntRangeForAllEnum()
        {
            foreach (var dtp in EnumValues)
            {
                var res = DatePartToRangeConverter.GetDatePartRange(dtp);
                if (dtp == DatePartEnum.Unknown)
                {
                    Assert.That(res, Is.Null, dtp.ToString());
                }
                else
                {
                    Assert.That(res, Is.Not.Null, dtp.ToString());
                }
            }
        }

        [Test]
        public void Test_DatePartToRangeConverter_GivesLengthForAllEnum()
        {
            foreach (var dtp in EnumValues)
            {
                var res = DatePartToRangeConverter.GetDateNameLengthEstimate(dtp);
                if (dtp == DatePartEnum.Unknown)
                {
                    Assert.That(res, Is.EqualTo(-1), dtp.ToString());
                }
                else
                {
                    Assert.That(res, Is.GreaterThan(0), dtp.ToString());
                }
            }
        }
    }
}
