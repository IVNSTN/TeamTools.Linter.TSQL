using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class TrimLeft : TrimmingFunction
    {
        private static readonly string FuncName = "LTRIM";
        private static readonly string RedundantDescr = "has no forward spaces";

        public TrimLeft() : base(FuncName, RedundantDescr)
        {
        }

        protected override string ModifyString(string originalValue, char[] trimmedChar)
            => originalValue.TrimStart(trimmedChar);
    }
}
