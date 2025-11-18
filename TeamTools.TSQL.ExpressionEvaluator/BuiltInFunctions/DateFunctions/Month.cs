using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class Month : SpecificDatePartFunction
    {
        private static readonly string FuncName = "MONTH";

        public Month() : base(FuncName, MonthRange, DatePartEnum.Month)
        {
        }

        public static SqlIntValueRange MonthRange { get; } = new SqlIntValueRange(1, 12);
    }
}
