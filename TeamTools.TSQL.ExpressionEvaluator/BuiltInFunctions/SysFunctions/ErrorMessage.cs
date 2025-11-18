using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ErrorMessage : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "ERROR_MESSAGE";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.NVarchar;
        private static readonly int Length = 4000;

        public ErrorMessage() : base(FuncName, ResultTypeName)
        {
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<ZeroArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlStrTypeValue>(base.DoEvaluateResultValue(call));
            if (value != null)
            {
                value = value.ChangeTo(Length, value.Source);
            }

            return value;
        }
    }
}
