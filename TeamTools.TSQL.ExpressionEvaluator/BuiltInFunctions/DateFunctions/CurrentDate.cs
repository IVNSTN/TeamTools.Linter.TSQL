using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class CurrentDate : CurrentMomentFunction<SqlDateOnlyValue>
    {
        private static readonly string FuncName = "CURRENT_DATE";
        private static readonly string OutputType = TSqlDomainAttributes.Types.Date;

        public CurrentDate() : base(FuncName, OutputType, TimeDetails.None, DateDetails.Full)
        {
        }

        protected override SqlDateOnlyValue ApplyNewRange(SqlDateOnlyValue value, SqlDateTimeValueRange newRange, SqlValueSource src)
            => value.ChangeTo(newRange, src);
    }
}
