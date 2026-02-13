using System;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlDateTimeRelativeValue : IComparable<SqlDateTimeRelativeValue>, IEquatable<SqlDateTimeRelativeValue>
    {
        public SqlDateTimeRelativeValue(TimeDetails timeAttributes, DateDetails dateAttributes) : this(DateTimeRangeKind.Unknown, timeAttributes, dateAttributes)
        { }

        public SqlDateTimeRelativeValue(DateTimeRangeKind rangeKind, DateDetails dateAttributes) : this(rangeKind, TimeDetails.None, dateAttributes)
        { }

        public SqlDateTimeRelativeValue(DateTimeRangeKind rangeKind, TimeDetails timeAttributes) : this(rangeKind, timeAttributes, DateDetails.None)
        { }

        public SqlDateTimeRelativeValue(DateTimeRangeKind rangeKind, TimeDetails timeAttributes, DateDetails dateAttributes)
        {
            RangeKind = rangeKind;
            TimeAttributes = timeAttributes;
            DateAttributes = dateAttributes;
        }

        public SqlDateTimeRelativeValue(TimeSpan value)
        {
            PreciseValue = new DateTime(value.Ticks);

            TimeAttributes = TimeDetails.RegularDateTime;
            DateAttributes = DateDetails.None;
        }

        public SqlDateTimeRelativeValue(DateTime value)
        {
            PreciseValue = value;
            RangeKind = DateTimeRangeKind.Precise;

            /*if (value > DateTime.Today)
            {
                RangeKind = DateTimeRangeKind.Precise;
            }
            else if (value < DateTime.Today)
            {
                RangeKind = DateTimeRangeKind.Past;
            }
            else // if (value > DateTime.Today)
            {
                // TODO : or precise?
                RangeKind = DateTimeRangeKind.Unknown;
            }
            */

            if (value.TimeOfDay == TimeSpan.Zero)
            {
                TimeAttributes = TimeDetails.None;
            }
            else
            {
                // TODO : shouldn't it come from the very specific value sql-datatype?
                TimeAttributes = TimeDetails.RegularDateTime;
            }

            if (value.Date == DateTime.MinValue)
            {
                DateAttributes = DateDetails.None;
            }
            else
            {
                DateAttributes = DateDetails.Full;
            }
        }

        public DateTimeRangeKind RangeKind { get; } = DateTimeRangeKind.Unknown;

        public TimeDetails TimeAttributes { get; } = TimeDetails.None;

        public DateDetails DateAttributes { get; } = DateDetails.None;

        public DateTime? PreciseValue { get; }

        public static bool operator <(SqlDateTimeRelativeValue a, SqlDateTimeRelativeValue b) => a.CompareTo(b) < 0;

        public static bool operator >(SqlDateTimeRelativeValue a, SqlDateTimeRelativeValue b) => a.CompareTo(b) > 0;

        public static bool operator >=(SqlDateTimeRelativeValue a, SqlDateTimeRelativeValue b) => a.CompareTo(b) >= 0;

        public static bool operator <=(SqlDateTimeRelativeValue a, SqlDateTimeRelativeValue b) => a.CompareTo(b) <= 0;

        public int CompareTo(SqlDateTimeRelativeValue other)
        {
            if (RangeKind == DateTimeRangeKind.Precise && other.RangeKind == DateTimeRangeKind.Precise)
            {
                return PreciseValue.Value.CompareTo(other.PreciseValue);
            }
            else if (RangeKind == DateTimeRangeKind.CurrentMoment && other.RangeKind == DateTimeRangeKind.CurrentMoment)
            {
                return 0;
            }
            else if (RangeKind == DateTimeRangeKind.Future && other.RangeKind != DateTimeRangeKind.Future)
            {
                return 1;
            }
            else if (RangeKind == DateTimeRangeKind.Past && other.RangeKind != DateTimeRangeKind.Past)
            {
                return -1;
            }
            else if (other.RangeKind == DateTimeRangeKind.Future && RangeKind != DateTimeRangeKind.Future)
            {
                return -1;
            }
            else if (other.RangeKind == DateTimeRangeKind.Past && RangeKind != DateTimeRangeKind.Past)
            {
                return 1;
            }

            return RangeKind - other.RangeKind;
        }

        public bool Equals(SqlDateTimeRelativeValue other)
        {
            return CompareTo(other) == 0;
        }
    }
}
