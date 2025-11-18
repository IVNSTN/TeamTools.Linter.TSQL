using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Evaluation;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Unicode : EncodeSymbolsAsNumbers
    {
        private static readonly string FuncName = "UNICODE";
        private static readonly string OutputType = TSqlDomainAttributes.Types.Int;

        public Unicode() : base(FuncName, OutputType)
        { }

        protected override SqlIntTypeValue ClarifySymbolCodeValue(SqlStrTypeValue str, SqlIntTypeValue res, EvaluationContext context)
            => res.ChangeTo(str.Value[0], str.Source);
    }
}
