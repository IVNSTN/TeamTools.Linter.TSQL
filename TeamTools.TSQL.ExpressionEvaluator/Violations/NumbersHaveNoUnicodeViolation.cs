using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class NumbersHaveNoUnicodeViolation : SqlViolation
    {
        public NumbersHaveNoUnicodeViolation(string sourceName, SqlValueSource source)
        : base(sourceName, source)
        {
        }
    }
}
