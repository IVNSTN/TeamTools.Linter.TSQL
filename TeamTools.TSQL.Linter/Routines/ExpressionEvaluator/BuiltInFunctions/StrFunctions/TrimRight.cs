namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class TrimRight : TrimmingFunction
    {
        private static readonly string FuncName = "RTRIM";
        private static readonly string RedundantDescr = "has no trailing spaces";

        public TrimRight() : base(FuncName, RedundantDescr)
        {
        }

        protected override string ModifyString(string originalValue, char[] trimmedChar)
            => originalValue.TrimEnd(trimmedChar);
    }
}
