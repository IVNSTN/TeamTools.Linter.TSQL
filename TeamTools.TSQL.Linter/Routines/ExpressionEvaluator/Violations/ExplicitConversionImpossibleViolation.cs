using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator
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
