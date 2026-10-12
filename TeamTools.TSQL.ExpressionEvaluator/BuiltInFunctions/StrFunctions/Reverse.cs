using System.Linq;
using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Properties;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Reverse : OriginalStringManipulation
    {
        private static readonly string FuncName = "REVERSE";
        private static readonly string RedundantDescr = Strings.ViolationDetails_BuiltInFunction_Reverse_StringIsPalindrome;

        public Reverse() : base(FuncName, RedundantDescr)
        {
        }

        protected override string ModifyString(string originalValue)
        {
            if (string.IsNullOrWhiteSpace(originalValue))
            {
                return originalValue;
            }

            if (originalValue.Length == 1)
            {
                return originalValue;
            }

            return new string(originalValue.Reverse().ToArray());
        }
    }
}
