using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Char : DecodeSymbolFromNumbers
    {
        private static readonly string OutputType = TSqlDomainAttributes.Types.Char;
        private static readonly string FuncName = "CHAR";
        private static readonly SqlIntValueRange CharCodeRange = new SqlIntValueRange(0, 255);

        public Char() : base(FuncName, OutputType)
        {
        }

        protected override SqlIntValueRange GetValidRange() => CharCodeRange;
    }
}
