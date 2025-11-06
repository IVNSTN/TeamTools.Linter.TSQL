namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Right : OriginalStringPartHandler
    {
        private static readonly string FuncName = "RIGHT";
        private static readonly int RequiredArgumentCount = 2;

        public Right() : base(FuncName, RequiredArgumentCount)
        {
        }

        protected override string TakeStringPartFrom(string srcValue, int startValue, int lenValue)
            => srcValue.Substring(srcValue.Length - lenValue);
    }
}
