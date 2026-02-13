using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class DateFromParts : BaseDateTimeFromParts
    {
        private static readonly int RequiredArgumentCount = 3;
        private static readonly string FuncName = "DATEFROMPARTS";
        private static readonly string OutputType = TSqlDomainAttributes.Types.Date;

        public DateFromParts() : base(FuncName, RequiredArgumentCount, OutputType, true, false)
        {
        }
    }
}
