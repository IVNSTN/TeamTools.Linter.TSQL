using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
{
    [ExcludeFromCodeCoverage]
    public class NullArithmeticsViolation : SqlViolation
    {
        public NullArithmeticsViolation(string sourceName, SqlValueSource source)
        : base(sourceName, source)
        {
        }
    }
}
