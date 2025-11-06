using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class RedundantFunctionArgumentViolation : SqlViolation
    {
        public RedundantFunctionArgumentViolation(string funcName, string argName, string descr, SqlValueSource source)
        : base($"{funcName} - {descr}. Argument can be omitted.", source)
        {
            FunctionName = funcName;
            ArgumentName = argName;
        }

        public string FunctionName { get; }

        public string ArgumentName { get; }
    }
}
