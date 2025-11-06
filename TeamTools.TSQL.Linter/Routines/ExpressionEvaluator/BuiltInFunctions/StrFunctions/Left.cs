namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.StrFunctions
{
    public class Left : OriginalStringPartHandler
    {
        private static readonly string FuncName = "LEFT";
        private static readonly int RequiredArgumentCount = 2;

        public Left() : base(FuncName, RequiredArgumentCount)
        {
        }

        protected override string TakeStringPartFrom(string srcValue, int startValue, int lenValue)
            => srcValue.Substring(0, lenValue);
    }
}
