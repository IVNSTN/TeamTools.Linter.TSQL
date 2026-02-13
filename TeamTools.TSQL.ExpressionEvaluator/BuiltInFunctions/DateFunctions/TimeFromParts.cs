using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class TimeFromParts : BaseDateTimeFromParts
    {
        private static readonly int RequiredArgumentCount = 5;
        private static readonly string FuncName = "TIMEFROMPARTS";
        private static readonly string OutputType = TSqlDomainAttributes.Types.Time;

        public TimeFromParts() : base(FuncName, RequiredArgumentCount, OutputType, false, true)
        {
        }
    }
}
