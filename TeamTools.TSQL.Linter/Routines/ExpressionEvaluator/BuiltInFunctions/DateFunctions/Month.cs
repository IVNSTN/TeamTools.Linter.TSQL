using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class Month : SpecificDatePartFunction
    {
        private static readonly string FuncName = "MONTH";

        public Month() : base(FuncName, MonthRange)
        {
        }

        public static SqlIntValueRange MonthRange { get; } = new SqlIntValueRange(1, 12);
    }
}
