using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Properties;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class NullArithmeticsViolation : SqlViolation
    {
        public NullArithmeticsViolation(string sourceName, SqlValueSource source)
        : base(string.Format(Strings.ViolationDetails_NullArithmeticsViolation_SourceIsNull, sourceName), source)
        {
        }
    }
}
