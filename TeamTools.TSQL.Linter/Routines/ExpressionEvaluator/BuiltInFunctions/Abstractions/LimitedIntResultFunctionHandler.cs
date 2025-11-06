using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public abstract class LimitedIntResultFunctionHandler : SqlZeroArgFunctionHandler
    {
        private static readonly string ResultTypeName = "dbo.INT";
        private readonly SqlIntValueRange resultRange;

        public LimitedIntResultFunctionHandler(string funcName, SqlIntValueRange resultRange)
        : this(funcName, resultRange, ResultTypeName)
        {
        }

        public LimitedIntResultFunctionHandler(string funcName, SqlIntValueRange resultRange, string resultType)
        : base(funcName, resultType)
        {
            this.resultRange = resultRange;
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<ZeroArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call));
            if (value is null)
            {
                return default;
            }

            return value.ChangeTo(resultRange, value.Source);
        }
    }
}
