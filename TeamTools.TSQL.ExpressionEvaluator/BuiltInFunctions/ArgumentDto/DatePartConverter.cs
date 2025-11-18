using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto
{
    public static class DatePartConverter
    {
        private static readonly Dictionary<string, string> DatePartNames;

        // TODO : import from SqlServerMetadata
        static DatePartConverter()
        {
            DatePartNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "HH",          "HH" },
                { "HOUR",        "HOUR" },
                { "N",           "N" },
                { "MI",          "MI" },
                { "MINUTE",      "MINUTE" },
                { "S",           "S" },
                { "SS",          "SS" },
                { "SECOND",      "SECOND" },
                { "MS",          "MS" },
                { "MILLISECOND", "MILLISECOND" },
                { "MCS",         "MCS" },
                { "MICROSECOND", "MICROSECOND" },
                { "NS",          "NS" },
                { "NANOSECOND",  "NANOSECOND" },
                { "TZ",          "TZ" },
                { "TZOFFSET",    "TZOFFSET" },
                { "YY",          "YY" },
                { "YYYY",        "YYYY" },
                { "YEAR",        "YEAR" },
                { "Q",           "Q" },
                { "QQ",          "QQ" },
                { "QUARTER",     "QUARTER" },
                { "M",           "M" },
                { "MM",          "MM" },
                { "MONTH",       "MONTH" },
                { "WK",          "WK" },
                { "WW",          "WW" },
                { "WEEK",        "WEEK" },
                { "Y",           "Y" },
                { "DY",          "DY" },
                { "DAYOFYEAR",   "DAYOFYEAR" },
                { "DW",          "DW" },
                { "W",           "W" },
                { "WEEKDAY",     "WEEKDAY" },
                { "D",           "D" },
                { "DD",          "DD" },
                { "DAY",         "DAY" },
                { "ISOWK",       "ISOWK" },
                { "ISOWW",       "ISOWW" },
                { "ISO_WEEK",    "ISO_WEEK" },
            };
        }

        public static IDictionary<string, string> SupportedDateParts => DatePartNames;

        // TODO : use aliases already defined in metadata
        public static DatePartEnum DatePartNameToEnumValue(string name)
        {
            switch (name)
            {
                case "HH":
                case "HOUR":
                    return DatePartEnum.Hour;

                case "N":
                case "MI":
                case "MINUTE":
                    return DatePartEnum.Minute;

                case "S":
                case "SS":
                case "SECOND":
                    return DatePartEnum.Second;

                case "YY":
                case "YYYY":
                case "YEAR":
                    return DatePartEnum.Year;

                case "Q":
                case "QQ":
                case "QUARTER":
                    return DatePartEnum.Quarter;

                case "M":
                case "MM":
                case "MONTH":
                    return DatePartEnum.Month;

                case "WK":
                case "WW":
                case "WEEK":
                case "ISOWK":
                case "ISOWW":
                case "ISO_WEEK":
                    return DatePartEnum.Week;

                case "Y":
                case "DY":
                case "DAYOFYEAR":
                    return DatePartEnum.DayOfYear;

                case "DW":
                case "W":
                case "WEEKDAY":
                    return DatePartEnum.DayOfWeek;

                case "D":
                case "DD":
                case "DAY":
                    return DatePartEnum.Day;

                case "MS":
                case "MILLISECOND":
                    return DatePartEnum.Millisecond;

                case "MCS":
                case "MICROSECOND":
                    return DatePartEnum.Microsecond;

                case "NS":
                case "NANOSECOND":
                    return DatePartEnum.Nanosecond;

                case "TZ":
                case "TZOFFSET":
                    return DatePartEnum.TimezoneOffset;
            }

            return DatePartEnum.Unknown;
        }
    }
}
