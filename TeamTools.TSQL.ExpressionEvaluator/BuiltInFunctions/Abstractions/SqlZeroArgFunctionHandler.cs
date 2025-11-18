using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    public abstract class SqlZeroArgFunctionHandler : SqlGenericFunctionHandler<SqlZeroArgFunctionHandler.ZeroArgs>
    {
        private readonly string resultTypeName;

        protected SqlZeroArgFunctionHandler(string funcName, string resultTypeName) : base(funcName, 0)
        {
            this.resultTypeName = resultTypeName;
        }

        public override sealed bool ValidateArgumentValues(CallSignature<ZeroArgs> call) => true;

        protected override sealed string DoEvaluateResultType(CallSignature<ZeroArgs> call) => resultTypeName;

        public class ZeroArgs
        {
        }
    }
}
