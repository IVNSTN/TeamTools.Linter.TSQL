using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class Year : SpecificDatePartFunction
    {
        private static readonly string FuncName = "YEAR";

        public Year() : base(FuncName, YearRange, DatePartEnum.Year)
        {
        }

        public static SqlIntValueRange YearRange { get; } = new SqlIntValueRange(0, 9999);
    }
}
