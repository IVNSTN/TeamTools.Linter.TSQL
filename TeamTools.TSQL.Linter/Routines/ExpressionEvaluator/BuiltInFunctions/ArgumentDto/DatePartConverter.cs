using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public static class DatePartConverter
    {
        private static readonly ICollection<string> DatePartNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        // TODO : import from SqlServerMetadata
        static DatePartConverter()
        {
            DatePartNames.Add("HH");
            DatePartNames.Add("HOUR");
            DatePartNames.Add("N");
            DatePartNames.Add("MI");
            DatePartNames.Add("MINUTE");
            DatePartNames.Add("S");
            DatePartNames.Add("SS");
            DatePartNames.Add("SECOND");
            DatePartNames.Add("MS");
            DatePartNames.Add("MILLISECOND");
            DatePartNames.Add("MCS");
            DatePartNames.Add("MICROSECOND");
            DatePartNames.Add("NS");
            DatePartNames.Add("NANOSECOND");
            DatePartNames.Add("TZ");
            DatePartNames.Add("TZOFFSET");
            DatePartNames.Add("YY");
            DatePartNames.Add("YYYY");
            DatePartNames.Add("YEAR");
            DatePartNames.Add("Q");
            DatePartNames.Add("QQ");
            DatePartNames.Add("QUARTER");
            DatePartNames.Add("M");
            DatePartNames.Add("MM");
            DatePartNames.Add("MONTH");
            DatePartNames.Add("WK");
            DatePartNames.Add("WW");
            DatePartNames.Add("WEEK");
            DatePartNames.Add("Y");
            DatePartNames.Add("DY");
            DatePartNames.Add("DAYOFYEAR");
            DatePartNames.Add("DW");
            DatePartNames.Add("W");
            DatePartNames.Add("WEEKDAY");
            DatePartNames.Add("D");
            DatePartNames.Add("DD");
            DatePartNames.Add("DAY");
            DatePartNames.Add("ISOWK");
            DatePartNames.Add("ISOWW");
            DatePartNames.Add("ISO_WEEK");
        }

        public static ICollection<string> SupportedDateParts => DatePartNames;

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
