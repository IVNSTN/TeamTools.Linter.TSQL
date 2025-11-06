namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class Cast : ExplicitConvertionFunction
    {
        private static readonly string FuncName = "CAST";

        public Cast() : base(FuncName)
        {
        }
    }
}
