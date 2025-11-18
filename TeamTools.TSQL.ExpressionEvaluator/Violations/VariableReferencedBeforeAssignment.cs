using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class VariableReferencedBeforeAssignment : SqlViolation
    {
        public VariableReferencedBeforeAssignment(string varName, SqlValueSource source)
        : base(varName, source)
        {
        }
    }
}
