using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
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
