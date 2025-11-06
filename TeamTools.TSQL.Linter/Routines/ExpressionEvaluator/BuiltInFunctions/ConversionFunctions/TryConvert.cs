namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class TryConvert : ExplicitConvertionFunction
    {
        private static readonly string FuncName = "TRY_CONVERT";

        public TryConvert() : base(FuncName)
        {
        }
    }
}
