using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class ExplicitConversionImpossibleViolation : SqlConversionViolation
    {
        public ExplicitConversionImpossibleViolation(string fromType, string toType, SqlValueSource source)
        : base(fromType, toType, source)
        {
        }
    }
}
