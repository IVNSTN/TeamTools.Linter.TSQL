using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Properties;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Upper : OriginalStringManipulation
    {
        private static readonly string FuncName = "UPPER";
        private static readonly string RedundantDescr = Strings.ViolationDetails_BuiltInFunction_Upper_StringIsInUpperCase;

        public Upper() : base(FuncName, RedundantDescr)
        {
        }

        protected override string ModifyString(string originalValue) => originalValue.ToUpper();
    }
}
