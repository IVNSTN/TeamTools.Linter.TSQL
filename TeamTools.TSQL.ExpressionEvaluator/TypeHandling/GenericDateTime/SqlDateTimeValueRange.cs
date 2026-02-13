using System;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlDateTimeValueRange : IComparable<SqlDateTimeValueRange>
    {
        public SqlDateTimeValueRange(SqlDateTimeRelativeValue lowerAndUpperRange) : this(lowerAndUpperRange, lowerAndUpperRange)
        { }

        public SqlDateTimeValueRange(DateTime lowerAndUpperDateTimeValue) : this(new SqlDateTimeRelativeValue(lowerAndUpperDateTimeValue))
        { }

        public SqlDateTimeValueRange(TimeSpan lowerAndUpperTimeValue) : this(new SqlDateTimeRelativeValue(lowerAndUpperTimeValue))
        { }

        public SqlDateTimeValueRange(SqlDateTimeRelativeValue lowerRange, SqlDateTimeRelativeValue upperRange)
        {
            Low = lowerRange;
            High = upperRange;
        }

        public SqlDateTimeRelativeValue Low { get; }

        public SqlDateTimeRelativeValue High { get; }

        public int CompareTo(SqlDateTimeValueRange other)
        {
            if (other.High == High && other.Low == Low)
            {
                return 0;
            }

            // TODO : shouldn't we respect Low value here?
            return other.High.CompareTo(High);
        }
    }
}
