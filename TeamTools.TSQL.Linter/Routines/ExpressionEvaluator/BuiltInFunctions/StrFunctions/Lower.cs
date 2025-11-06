namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Lower : OriginalStringManipulation
    {
        private static readonly string FuncName = "LOWER";
        private static readonly string RedundantDescr = "is already lowercase";

        public Lower() : base(FuncName, RedundantDescr)
        {
        }

        protected override string ModifyString(string originalValue) => originalValue.ToLower();
    }
}
