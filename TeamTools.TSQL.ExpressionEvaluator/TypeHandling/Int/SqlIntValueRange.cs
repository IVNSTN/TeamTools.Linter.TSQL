using TeamTools.TSQL.ExpressionEvaluator.TypeHandling.GenericNumber;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlIntValueRange : SqlGenericNumberValueRange<int>
    {
        public SqlIntValueRange(int low, int high) : base(low, high)
        {
        }

        public static SqlIntValueRange RevertRange(SqlIntValueRange range)
        {
            return new SqlIntValueRange(-range.High, -range.Low);
        }

        public bool IsValueWithin(int value) => value >= Low && value <= High;
    }
}
