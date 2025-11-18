using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class TryConvert : ExplicitConvertionFunction
    {
        private static readonly string FuncName = "TRY_CONVERT";

        public TryConvert() : base(FuncName)
        {
        }
    }
}
