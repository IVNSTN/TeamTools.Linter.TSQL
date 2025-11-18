using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class TryCast : ExplicitConvertionFunction
    {
        private static readonly string FuncName = "TRY_CAST";

        public TryCast() : base(FuncName)
        {
        }
    }
}
