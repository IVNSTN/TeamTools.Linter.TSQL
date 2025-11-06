using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class Day : SpecificDatePartFunction
    {
        private static readonly string FuncName = "DAY";

        public Day() : base(FuncName, DayRange)
        {
        }

        public static SqlIntValueRange DayRange { get; } = new SqlIntValueRange(1, 31);
    }
}
