using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    // TODO : support leading/trailing/both option
    public class Trim : TrimmingFunction
    {
        private static readonly string FuncName = "TRIM";
        // TODO : move to resources and translate
        // also respect other symbols - it support not spaces only
        private static readonly string RedundantDescr = "has no spaces around";

        public Trim() : base(FuncName, RedundantDescr)
        {
        }

        protected override string ModifyString(string originalValue, char[] trimmedChar)
            => originalValue.Trim(trimmedChar);
    }
}
