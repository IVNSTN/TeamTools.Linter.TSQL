using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class SmallDateTimeFromParts : BaseDateTimeFromParts
    {
        private static readonly int RequiredArgumentCount = 5;
        private static readonly string FuncName = "SMALLDATETIMEFROMPARTS";
        private static readonly string OutputType = TSqlDomainAttributes.Types.SmallDateTime;

        public SmallDateTimeFromParts() : base(FuncName, RequiredArgumentCount, OutputType, false, true)
        {
        }
    }
}
