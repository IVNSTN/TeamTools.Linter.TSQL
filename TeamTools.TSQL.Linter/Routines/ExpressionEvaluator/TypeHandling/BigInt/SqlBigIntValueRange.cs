using System.Numerics;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public class SqlBigIntValueRange : SqlGenericNumberValueRange<BigInteger>
    {
        public SqlBigIntValueRange(BigInteger low, BigInteger high) : base(low, high)
        {
        }

        public static SqlBigIntValueRange RevertRange(SqlBigIntValueRange range)
        {
            return new SqlBigIntValueRange(-range.High, -range.Low);
        }

        public bool IsValueWithin(BigInteger value) => value >= Low && value <= High;
    }
}
