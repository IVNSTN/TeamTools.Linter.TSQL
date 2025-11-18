using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class Cast : ExplicitConvertionFunction
    {
        private static readonly string FuncName = "CAST";

        public Cast() : base(FuncName)
        {
        }
    }
}
