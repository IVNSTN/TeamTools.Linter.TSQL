using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class Convert : ExplicitConvertionFunction
    {
        private static readonly string FuncName = "CONVERT";

        public Convert() : base(FuncName)
        {
        }
    }
}
