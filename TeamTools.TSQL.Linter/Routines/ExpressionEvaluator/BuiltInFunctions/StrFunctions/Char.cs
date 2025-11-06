using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Char : DecodeSymbolFromNumbers
    {
        private static readonly string OutputType = "dbo.CHAR";
        private static readonly string FuncName = "CHAR";
        private static readonly SqlIntValueRange CharCodeRange = new SqlIntValueRange(0, 255);

        public Char() : base(FuncName, OutputType)
        {
        }

        protected override SqlIntValueRange GetValidRange() => CharCodeRange;
    }
}
