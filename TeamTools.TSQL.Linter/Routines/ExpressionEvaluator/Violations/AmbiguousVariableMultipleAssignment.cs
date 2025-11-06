using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
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
