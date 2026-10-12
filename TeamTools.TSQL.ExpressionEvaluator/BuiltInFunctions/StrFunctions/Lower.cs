using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Properties;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Lower : OriginalStringManipulation
    {
        private static readonly string FuncName = "LOWER";
        private static readonly string RedundantDescr = Strings.ViolationDetails_BuiltInFunction_Upper_StringIsInLowerCase;

        public Lower() : base(FuncName, RedundantDescr)
        {
        }

        protected override string ModifyString(string originalValue) => originalValue.ToLower();
    }
}
