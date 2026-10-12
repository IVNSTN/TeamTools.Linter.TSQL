using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class TrimLeft : TrimmingFunction
    {
        private static readonly string FuncName = "LTRIM";
        // TODO : move to resources and translate
        // also respect other symbols - it support not spaces only
        private static readonly string RedundantDescr = "has no forward spaces";

        public TrimLeft() : base(FuncName, RedundantDescr)
        {
        }

        protected override string ModifyString(string originalValue, char[] trimmedChar)
            => originalValue.TrimStart(trimmedChar);
    }
}
