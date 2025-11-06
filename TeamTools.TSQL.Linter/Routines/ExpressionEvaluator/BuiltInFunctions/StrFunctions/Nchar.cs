using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Nchar : DecodeSymbolFromNumbers
    {
        private static readonly string OutputType = "dbo.NCHAR";
        private static readonly string FuncName = "NCHAR";
        private static readonly SqlIntValueRange CharCodeRange = new SqlIntValueRange(0, int.MaxValue);

        public Nchar() : base(FuncName, OutputType)
        {
        }

        protected override SqlIntValueRange GetValidRange() => CharCodeRange;
    }
}
