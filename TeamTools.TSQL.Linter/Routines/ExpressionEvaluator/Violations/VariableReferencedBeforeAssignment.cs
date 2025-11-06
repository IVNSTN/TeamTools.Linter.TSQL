using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
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
