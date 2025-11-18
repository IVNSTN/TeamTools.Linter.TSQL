using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class Day : SpecificDatePartFunction
    {
        private static readonly string FuncName = "DAY";

        public Day() : base(FuncName, DayRange, DatePartEnum.Day)
        {
        }

        public static SqlIntValueRange DayRange { get; } = new SqlIntValueRange(1, 31);
    }
}
