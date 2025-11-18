using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public static class DatePartToRangeConverter
    {
        public static SqlIntValueRange GetDatePartRange(DatePartEnum datePart)
        {
            switch (datePart)
            {
                case DatePartEnum.Year:
                    return Year.YearRange;
                case DatePartEnum.Quarter:
                    return new SqlIntValueRange(1, 4);
                case DatePartEnum.Month:
                    return Month.MonthRange;
                case DatePartEnum.Week:
                    return new SqlIntValueRange(1, 53);
                case DatePartEnum.Day:
                    return Day.DayRange;
                case DatePartEnum.DayOfWeek:
                    return new SqlIntValueRange(1, 7);
                case DatePartEnum.DayOfYear:
                    return new SqlIntValueRange(1, 366);
                case DatePartEnum.Hour:
                    return new SqlIntValueRange(0, 23);
                case DatePartEnum.Minute:
                    return new SqlIntValueRange(0, 59);
                case DatePartEnum.Second:
                    return new SqlIntValueRange(0, 59);
                case DatePartEnum.Millisecond:
                    return new SqlIntValueRange(0, 999);
                case DatePartEnum.Microsecond:
                    return new SqlIntValueRange(0, 999999);
                case DatePartEnum.Nanosecond:
                    return new SqlIntValueRange(0, 999999999);
                case DatePartEnum.TimezoneOffset:
                    return new SqlIntValueRange(-840, +840);
                default:
                    return default;
            }
        }

        public static int GetDateNameLengthEstimate(DatePartEnum datePart)
        {
            switch (datePart)
            {
                case DatePartEnum.Year:
                    return 4;
                case DatePartEnum.Quarter:
                    return 1;
                case DatePartEnum.Month:
                    // something approximate because the result
                    // depends on current language
                    return 20;
                case DatePartEnum.Week:
                    return 2;
                case DatePartEnum.Day:
                    return 2;
                case DatePartEnum.DayOfWeek:
                    // something approximate because the result
                    // depends on current language
                    return 20;
                case DatePartEnum.DayOfYear:
                    return 3;
                case DatePartEnum.Hour:
                    return 2;
                case DatePartEnum.Minute:
                    return 2;
                case DatePartEnum.Second:
                    return 2;
                case DatePartEnum.Millisecond:
                    return 3;
                case DatePartEnum.Microsecond:
                    return 6;
                case DatePartEnum.Nanosecond:
                    return 9;
                case DatePartEnum.TimezoneOffset:
                    // -14:00 - +14:00
                    return 6;
                default:
                    return -1;
            }
        }
    }
}
