using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class UnableToConvertViolation : SqlConversionViolation
    {
        public UnableToConvertViolation(string value, string targetType, SqlValueSource source)
        : base(value, targetType, source)
        {
        }
    }
}
