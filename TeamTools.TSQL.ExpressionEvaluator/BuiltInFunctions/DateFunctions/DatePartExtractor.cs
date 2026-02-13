using System;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public static class DatePartExtractor
    {
        public static bool ExtractDatePartFromSpecificDate(DateTime date, DatePartEnum datePart, out int datePartValue)
        {
            datePartValue = 0;

            if (date.Equals(DateTime.MinValue))
            {
                return true;
            }

            switch (datePart)
            {
                case DatePartEnum.Year:
                    datePartValue = date.Year;
                    break;

                case DatePartEnum.Month:
                    datePartValue = date.Month;
                    break;

                case DatePartEnum.Day:
                    datePartValue = date.Day;
                    break;

                case DatePartEnum.Quarter:
                    datePartValue = 1 + ((date.Month - 1) / 3);
                    break;

                case DatePartEnum.Week:
                    datePartValue = 1 + ((date.DayOfYear - 1) / 7);
                    break;

                case DatePartEnum.DayOfYear:
                    datePartValue = date.DayOfYear;
                    break;

                case DatePartEnum.DayOfWeek:
                    datePartValue = (int)date.DayOfWeek; // TODO : respect current culture and DateFirst
                    break;

                case DatePartEnum.Hour:
                    datePartValue = date.Hour;
                    break;

                case DatePartEnum.Minute:
                    datePartValue = date.Minute;
                    break;

                case DatePartEnum.Second:
                    datePartValue = date.Second;
                    break;

                case DatePartEnum.Millisecond:
                    datePartValue = date.Millisecond;
                    break;

#if NET8_0_OR_GREATER
                case DatePartEnum.Microsecond:
                    datePartValue = date.Microsecond;
                    break;

                case DatePartEnum.Nanosecond:
                    datePartValue = date.Nanosecond;
                    break;
#endif

                default: return false;
            }

            return true;
        }
    }
}
