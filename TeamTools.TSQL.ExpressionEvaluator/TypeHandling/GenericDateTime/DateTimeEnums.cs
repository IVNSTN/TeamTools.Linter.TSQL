using System;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public enum DateTimeRangeKind
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Precise date and/or time value
        /// </summary>
        Precise,

        /// <summary>
        /// CurrentMoment - offset
        /// </summary>
        Past,

        /// <summary>
        /// Relative date or time: current date / time
        /// </summary>
        CurrentMoment,

        /// <summary>
        /// CurrentMoment + offset
        /// </summary>
        Future,
    }

    [Flags]
    public enum TimeDetails
    {
        /// <summary>
        /// No time info (date only)
        /// </summary>
        None = 0,

        /// <summary>
        /// Contains hours
        /// </summary>
        Hours = 1,

        /// <summary>
        /// Contains minutes
        /// </summary>
        Minutes = 2,

        /// <summary>
        /// Contains seconds
        /// </summary>
        Seconds = 4,

        /// <summary>
        /// Contains milliseconds
        /// </summary>
        Milliseconds = 8,

        /// <summary>
        /// Contains microseconds
        /// </summary>
        Microseconds = 16,

        /// <summary>
        /// HH MM SS MS
        /// </summary>
        RegularDateTime = Hours | Minutes | Seconds | Milliseconds,

        /// <summary>
        /// HH MM SS MS NS
        /// </summary>
        Detailed = Hours | Minutes | Seconds | Milliseconds | Microseconds,

        /// <summary>
        /// HH MM
        /// </summary>
        DayTime = Hours | Minutes,

        /// <summary>
        /// No time info (for date-only values)
        /// </summary>
        DateOnly = None,
    }

    // TODO : shouldn't it be buils as Flags similarly to TimeDetails?
    public enum DateDetails
    {
        /// <summary>
        /// No date info (time only)
        /// </summary>
        None = 0,

        /// <summary>
        /// Small date info with limited range
        /// </summary>
        Small,

        /// <summary>
        /// Full date info
        /// </summary>
        Full,
    }
}
