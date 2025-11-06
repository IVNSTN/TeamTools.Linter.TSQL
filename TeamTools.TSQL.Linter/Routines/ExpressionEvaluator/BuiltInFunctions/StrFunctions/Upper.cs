namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Upper : OriginalStringManipulation
    {
        private static readonly string FuncName = "UPPER";
        private static readonly string RedundantDescr = "is already uppercase";

        public Upper() : base(FuncName, RedundantDescr)
        {
        }

        protected override string ModifyString(string originalValue) => originalValue.ToUpper();
    }
}
