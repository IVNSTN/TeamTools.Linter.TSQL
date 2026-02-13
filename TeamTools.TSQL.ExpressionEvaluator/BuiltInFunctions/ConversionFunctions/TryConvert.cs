using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class TryConvert : ExplicitConvertionFunction
    {
        private static readonly string FuncName = "TRY_CONVERT";
        private static readonly int MinArgumentCount = 2;
        private static readonly int MaxArgumentCount = 3; // With conversion Style provided

        public TryConvert() : base(FuncName, MinArgumentCount, MaxArgumentCount)
        {
        }
    }
}
