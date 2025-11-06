using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class DivideByZeroViolation : SqlViolation
    {
        public DivideByZeroViolation(SqlValueSource source)
        : base($"{source} is 0 here", source)
        {
        }
    }
}
