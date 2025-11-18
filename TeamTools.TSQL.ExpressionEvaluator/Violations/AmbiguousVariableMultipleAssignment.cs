using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class AmbiguousVariableMultipleAssignment : SqlViolation
    {
        public AmbiguousVariableMultipleAssignment(string varName, SqlValueSource source)
        : base(varName, source)
        {
        }
    }
}
