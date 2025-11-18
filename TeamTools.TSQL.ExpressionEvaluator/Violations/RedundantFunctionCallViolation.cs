using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
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
