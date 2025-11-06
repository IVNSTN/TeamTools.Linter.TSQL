namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
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
