using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class HostId : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "HOST_ID";
        private static readonly string ResultTypeName = "dbo.CHAR";
        private static readonly int Length = 10;

        public HostId() : base(FuncName, ResultTypeName)
        {
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<ZeroArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(base.DoEvaluateResultValue(call));
            if (value is null)
            {
                return default;
            }

            return value.ChangeTo(Length, value.Source);
        }
    }
}
