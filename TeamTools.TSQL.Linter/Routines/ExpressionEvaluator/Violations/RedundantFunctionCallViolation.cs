using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class RedundantFunctionCallViolation : SqlViolation
    {
        public RedundantFunctionCallViolation(string funcName, string descr, SqlValueSource source)
        : base($"{funcName} - {descr}", source)
        {
            FunctionName = funcName;
        }

        public string FunctionName { get; }
    }
}
