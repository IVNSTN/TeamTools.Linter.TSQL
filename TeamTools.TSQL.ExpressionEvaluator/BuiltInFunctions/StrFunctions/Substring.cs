using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Substring : OriginalStringPartHandler
    {
        private static readonly string FuncName = "SUBSTRING";
        private static readonly int RequiredArgumentCount = 3;

        public Substring() : base(FuncName, RequiredArgumentCount)
        {
        }

        protected override string TakeStringPartFrom(string srcValue, int startValue, int lenValue)
            => srcValue.Substring(startValue - 1, lenValue);
    }
}
