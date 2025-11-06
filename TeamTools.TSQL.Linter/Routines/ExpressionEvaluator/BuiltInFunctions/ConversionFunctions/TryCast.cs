namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class TryCast : ExplicitConvertionFunction
    {
        private static readonly string FuncName = "TRY_CAST";

        public TryCast() : base(FuncName)
        {
        }
    }
}
