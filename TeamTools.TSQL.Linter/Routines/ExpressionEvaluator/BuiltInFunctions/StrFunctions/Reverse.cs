using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Reverse : OriginalStringManipulation
    {
        private static readonly string FuncName = "REVERSE";
        private static readonly string RedundantDescr = "is palindrome";

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
