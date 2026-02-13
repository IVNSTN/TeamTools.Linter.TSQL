using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class GetDate : CurrentMomentFunction<SqlDateTimeValue>
    {
        private static readonly string FuncName = "GETDATE";
        private static readonly string OutputType = TSqlDomainAttributes.Types.DateTime;

        public GetDate() : base(FuncName, OutputType, TimeDetails.RegularDateTime, DateDetails.Full)
        {
        }

        protected override SqlDateTimeValue ApplyNewRange(SqlDateTimeValue value, SqlDateTimeValueRange newRange, SqlValueSource src)
            => value.ChangeTo(newRange, src);
    }
}
