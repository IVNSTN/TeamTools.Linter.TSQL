using System;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public abstract class SqlGenericNumberValueRange<TNumber> : IComparable<SqlGenericNumberValueRange<TNumber>>,
        IEquatable<SqlGenericNumberValueRange<TNumber>>
    where TNumber : IComparable, IComparable<TNumber>, IEquatable<TNumber>
    {
        public SqlGenericNumberValueRange(TNumber low, TNumber high)
        {
            Low = low;
            High = high;
        }

        public TNumber Low { get; }

        public TNumber High { get; }

        public static int Compare(SqlGenericNumberValueRange<TNumber> a, SqlGenericNumberValueRange<TNumber> b)
        {
            if (a.Low.Equals(b.Low) && a.High.Equals(b.High))
            {
                return 0;
            }

            // a is wider
            if ((a.Low.CompareTo(b.Low) <= 0) && (a.High.CompareTo(b.High) >= 0))
            {
                return 1;
            }

            // b is wider
            if ((b.Low.CompareTo(a.Low) <= 0) && (b.High.CompareTo(a.High) >= 0))
            {
                return -1;
            }

            // higher bound wins
            return a.High.CompareTo(b.High);
        }

        public int CompareTo(SqlGenericNumberValueRange<TNumber> other) => Compare(this, other);

        public bool Equals(SqlGenericNumberValueRange<TNumber> other) => 0 == CompareTo(other);
    }
}
