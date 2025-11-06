namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public abstract class SqlZeroArgFunctionHandler : SqlGenericFunctionHandler<SqlZeroArgFunctionHandler.ZeroArgs>
    {
        private readonly string resultTypeName;

        public SqlZeroArgFunctionHandler(string funcName, string resultTypeName) : base(funcName, 0)
        {
            this.resultTypeName = resultTypeName;
        }

        public override sealed bool ValidateArgumentValues(CallSignature<ZeroArgs> call) => true;

        protected override string DoEvaluateResultType(CallSignature<ZeroArgs> call) => resultTypeName;

        public class ZeroArgs
        {
        }
    }
}
