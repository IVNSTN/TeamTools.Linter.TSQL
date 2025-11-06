namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.ConversionFunctions
{
    public class Convert : ExplicitConvertionFunction
    {
        private static readonly string FuncName = "CONVERT";

        public Convert() : base(FuncName)
        {
        }
    }
}
