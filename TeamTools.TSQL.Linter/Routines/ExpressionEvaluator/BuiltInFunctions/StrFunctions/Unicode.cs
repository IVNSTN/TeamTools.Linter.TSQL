using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Unicode : EncodeSymbolsAsNumbers
    {
        private static readonly string OutputType = "dbo.INT";
        private static readonly string FuncName = "UNICODE";

        public Unicode() : base(FuncName, OutputType)
        { }

        protected override SqlIntTypeValue ClarifySymbolCodeValue(SqlStrTypeValue str, SqlIntTypeValue res, EvaluationContext context)
            => res.ChangeTo(str.Value[0], str.Source);
    }
}
