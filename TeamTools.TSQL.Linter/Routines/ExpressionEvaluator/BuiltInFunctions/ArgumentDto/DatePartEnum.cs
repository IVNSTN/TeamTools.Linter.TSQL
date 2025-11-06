namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public enum DatePartEnum
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Year
        /// </summary>
        Year,

        /// <summary>
        /// Quarter
        /// </summary>
        Quarter,

        /// <summary>
        /// Month of year
        /// </summary>
        Month,

        /// <summary>
        /// Week number of a year
        /// </summary>
        Week,

        /// <summary>
        /// Day of month
        /// </summary>
        Day,

        /// <summary>
        /// Hours
        /// </summary>
        Hour,

        /// <summary>
        /// Minutes
        /// </summary>
        Minute,

        /// <summary>
        /// Seconds
        /// </summary>
        Second,

        /// <summary>
        /// Day of week
        /// </summary>
        DayOfWeek,

        /// <summary>
        /// Day of year
        /// </summary>
        DayOfYear,

        /// <summary>
        /// Millisecond (3 digit)
        /// </summary>
        Millisecond,

        /// <summary>
        /// Microsecond (6 digits)
        /// </summary>
        Microsecond,

        /// <summary>
        /// Nanosecond (9 digits)
        /// </summary>
        Nanosecond,

        /// <summary>
        /// Timezone offset
        /// </summary>
        TimezoneOffset,
    }
}
