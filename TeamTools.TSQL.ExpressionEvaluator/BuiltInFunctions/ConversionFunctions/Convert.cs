using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class Convert : ExplicitConvertionFunction
    {
        private static readonly string FuncName = "CONVERT";
        private static readonly int MinArgumentCount = 2;
        private static readonly int MaxArgumentCount = 3; // With conversion Style provided

        public Convert() : base(FuncName, MinArgumentCount, MaxArgumentCount)
        {
        }
    }
}
