using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class NationalSymbolFailureViolation : SqlViolation
    {
        public NationalSymbolFailureViolation(string sourceLiteral, SqlValueSource source)
        : base(sourceLiteral, source)
        {
        }
    }
}
