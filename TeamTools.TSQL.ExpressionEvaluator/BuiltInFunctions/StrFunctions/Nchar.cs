using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Nchar : DecodeSymbolFromNumbers
    {
        private static readonly string OutputType = TSqlDomainAttributes.Types.NChar;
        private static readonly string FuncName = "NCHAR";
        private static readonly SqlIntValueRange CharCodeRange = new SqlIntValueRange(0, int.MaxValue);

        public Nchar() : base(FuncName, OutputType)
        {
        }

        protected override SqlIntValueRange GetValidRange() => CharCodeRange;
    }
}
