using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class Year : SpecificDatePartFunction
    {
        private static readonly string FuncName = "YEAR";

        public Year() : base(FuncName, YearRange)
        {
        }

        public static SqlIntValueRange YearRange { get; } = new SqlIntValueRange(0, 9999);
    }
}
