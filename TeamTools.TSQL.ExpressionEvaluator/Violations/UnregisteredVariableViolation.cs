using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class UnregisteredVariableViolation : SqlViolation
    {
        public UnregisteredVariableViolation(string varName, SqlValueSource source)
        : base(varName, source)
        {
        }
    }
}
