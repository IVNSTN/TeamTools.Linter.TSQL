using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling.GenericNumber;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlDecimalValueRange : SqlGenericNumberValueRange<decimal>
    {
        public SqlDecimalValueRange(decimal low, decimal high, int precision, int scale) : base(low, high)
        {
            Precision = precision;
            Scale = scale;

            if (Precision > TSqlDomainAttributes.MaxDecimalPrecision)
            {
                int delta = Precision - TSqlDomainAttributes.MaxDecimalPrecision;
                Precision = TSqlDomainAttributes.MaxDecimalPrecision;
                Scale -= delta;
            }

            if (Scale < 0)
            {
                Scale = 0;
            }
            else if (Scale > Precision)
            {
                // TODO : not sure what for -1
                Scale = Precision - 1;
            }
        }

        public int Precision { get; set; }

        public int Scale { get; set; }

        public static SqlDecimalValueRange RevertRange(SqlDecimalValueRange range)
        {
            return new SqlDecimalValueRange(-range.High, -range.Low, range.Precision, range.Scale);
        }

        // TODO : check match for scale and precision
        public bool IsValueWithin(decimal value) => value >= Low && value <= High;

        public virtual bool Equals(SqlDecimalValueRange other)
        {
            return base.Equals(other)
                && Precision.Equals(other.Precision)
                && Scale.Equals(other.Scale);
        }
    }
}
