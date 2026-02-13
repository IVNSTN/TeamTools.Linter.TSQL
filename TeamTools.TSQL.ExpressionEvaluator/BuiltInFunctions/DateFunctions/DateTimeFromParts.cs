using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class DateTimeFromParts : BaseDateTimeFromParts
    {
        private static readonly int RequiredArgumentCount = 7;
        private static readonly string FuncName = "DATETIMEFROMPARTS";
        private static readonly string OutputType = TSqlDomainAttributes.Types.DateTime;

        public DateTimeFromParts() : base(FuncName, RequiredArgumentCount, OutputType, true, true)
        {
        }
    }
}
