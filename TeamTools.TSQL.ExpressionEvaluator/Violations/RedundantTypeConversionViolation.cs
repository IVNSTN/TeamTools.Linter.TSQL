using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class RedundantTypeConversionViolation : SqlViolation
    {
        public RedundantTypeConversionViolation(string message, SqlValueSource source)
        : base(message, source)
        {
        }
    }
}
