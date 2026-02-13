using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.DateFunctions
{
    public class SysDateTime : CurrentMomentFunction<SqlDateTimeValue>
    {
        private static readonly string FuncName = "SYSDATETIME";
        private static readonly string OutputType = TSqlDomainAttributes.Types.DateTime2;

        public SysDateTime() : base(FuncName, OutputType, TimeDetails.Detailed, DateDetails.Full)
        {
        }

        protected override SqlDateTimeValue ApplyNewRange(SqlDateTimeValue value, SqlDateTimeValueRange newRange, SqlValueSource src)
            => value.ChangeTo(newRange, src);
    }
}
