using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Violations
{
    [ExcludeFromCodeCoverage]
    public class ImplicitConversionImpossibleViolation : SqlConversionViolation
    {
        public ImplicitConversionImpossibleViolation(string fromType, string toType, SqlValueSource source)
        : base(fromType, toType, source)
        {
        }
    }
}
