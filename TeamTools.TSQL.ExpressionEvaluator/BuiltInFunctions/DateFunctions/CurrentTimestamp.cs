using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class CurrentTimestamp : CurrentMomentFunction<SqlDateTimeValue>
    {
        private static readonly string FuncName = "CURRENT_TIMESTAMP";
        private static readonly string OutputType = TSqlDomainAttributes.Types.DateTime;

        public CurrentTimestamp() : base(FuncName, OutputType, TimeDetails.RegularDateTime, DateDetails.Full)
        {
        }

        protected override SqlDateTimeValue ApplyNewRange(SqlDateTimeValue value, SqlDateTimeValueRange newRange, SqlValueSource src)
            => value.ChangeTo(newRange, src);
    }
}
