using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Properties;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class DivideByZeroViolation : SqlViolation
    {
        public DivideByZeroViolation(SqlValueSource source)
        : base(string.Format(Strings.ViolationDetails_DivideByZeroViolation_SourceIsZero, source), source)
        {
        }
    }
}
